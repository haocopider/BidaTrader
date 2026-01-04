using BidaTrader.Server.Helpers;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class ProductService : ServerService<ProductDto>
    {
        private readonly IWebHostEnvironment _env;
        public ProductService(AppDbContext context, IWebHostEnvironment env) : base(context) {
            _env = env;
        }

        public override async Task<ProductDto?> GetItemByIdAsync(int id)
        {
            var p = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return null;

            return new ProductDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = p.ProductImages?.FirstOrDefault(i => i.IsMain)?.ImageUrl
                           ?? p.ProductImages?.FirstOrDefault()?.ImageUrl,

                Images = p.ProductImages?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
                Price = p.Price,
                Quantity = p.Quantity,
                Rating = p.Rating,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                StoreId = p.StoreId,
                BrandId = p.BrandId,
                StoreLogo = p.Store?.LogoUrl,
                StoreName = p.Store?.StoreName,
                BrandName = p.Brand?.Name ?? "Không có thương hiệu"
            };
        }

        public async Task<bool> CreateProductAsync(ProductCreateUpdateDto dto, int accountId)
        {
            try
            {
                // 1. Kiểm tra Store
                var store = await _context.Stores.FirstOrDefaultAsync(s => s.AccountId == accountId);
                if (store == null)
                {
                    Console.WriteLine($"[ERROR] Account {accountId} không có quyền tạo SP vì không tìm thấy Store tương ứng.");
                    return false;
                }

                // 2. Khởi tạo Product
                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Quantity = dto.Quantity ?? 1,
                    CategoryId = dto.CategoryId,
                    BrandId = dto.BrandId,
                    StoreId = store.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true,
                    Rating = 5,
                    ProductImages = new List<ProductImage>() // Đảm bảo không bị Null
                };

                // 3. Xử lý ảnh
                if (dto.Images != null && dto.Images.Any())
                {
                    for (int i = 0; i < dto.Images.Count; i++)
                    {
                        string imageUrl = await SaveImageToFolder(dto.Images[i], store.StoreName);

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            product.ProductImages.Add(new ProductImage
                            {
                                ImageUrl = imageUrl,
                                IsMain = (i == 0)
                            });
                        }
                    }
                }

                await _context.Products.AddAsync(product);

                // 4. Lưu và kiểm tra
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Log chi tiết để bạn copy paste vào đây nếu vẫn lỗi
                Console.WriteLine($"[CRITICAL ERROR]: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[INNER ERROR]: {ex.InnerException.Message}");
                return false;
            }
        }

        public override async Task<bool> UpdateItemAsync(ProductDto dto)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == dto.Id);

                if (product == null) return false;

                // --- Update thông tin cơ bản ---
                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.Quantity = dto.Quantity ?? 1;
                product.CategoryId = dto.CategoryId;
                product.BrandId = dto.BrandId;
                product.UpdatedAt = DateTime.UtcNow;

                if (dto.Images != null && dto.Images.Any())
                {
                    foreach (var oldImage in product.ProductImages)
                    {
                        DeleteImageFile(oldImage.ImageUrl);
                    }

                    _context.ProductImages.RemoveRange(product.ProductImages);
                    product.ProductImages.Clear();

                    for (int i = 0; i < dto.Images.Count; i++)
                    {
                        var imageUrl = await SaveImageToFolder(dto.Images[i], dto.Name);

                        product.ProductImages.Add(new ProductImage
                        {
                            ImageUrl = imageUrl,
                            IsMain = (i == 0)
                        });
                    }
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update Product Error: {ex}");
                return false;
            }
        }

        public override async Task<bool> DeleteItemAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                _context.Products.Remove(product);
                // Do có quan hệ Cascade Delete nên ProductImages sẽ tự động bị xóa theo
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ProductPerPage> GetMyStore(int accountId, int pageIndex, int pageSize)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (store == null)
                throw new Exception("Store không tồn tại");

            var query = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Where(p => p.StoreId == store.Id)
                .AsQueryable();

            int totalCount = await query.CountAsync();
            var products = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                Name = p.Name,
                ImageUrl = p.ProductImages?
                            .Where(i => i.IsMain)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault(),
                Price = p.Price,
                Quantity = p.Quantity,
                Rating = p.Rating,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                StoreId = p.StoreId,
                BrandId = p.BrandId ?? null,
                StoreLogo = p.Store.LogoUrl,
                StoreName = p.Store.StoreName
            }).ToList();

            return new ProductPerPage
            {
                Items = dtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ProductPerPage> GetProductsForHomePageAsync(
            int? categoryId,
            int? brandId,
            string? pname,
            string? sname,
            decimal? minPrice,
            decimal? maxPrice,
            bool? latest,
            bool? highest,
            float? rating,
            int pageIndex,
            int pageSize)
        {

            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Store)
                .AsQueryable();

            if (categoryId > 0)
                query = query.Where(p => p.CategoryId == categoryId);

            if (brandId > 0)
                query = query.Where(p => p.BrandId == brandId);

            if (!string.IsNullOrWhiteSpace(pname))
                query = query.Where(p => p.Name.Contains(pname));

            if (!string.IsNullOrWhiteSpace(sname))
                query = query.Where(p => p.Store.StoreName.Contains(sname));

            if (rating > 0)
                query = query.Where(p => p.Rating >= rating);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (highest == true)
                query = query.OrderByDescending(p => p.Price);
            else if (latest == true)
                query = query.OrderByDescending(p => p.CreatedAt);
            else
                query = query.OrderByDescending(p => p.Id);

            int totalCount = await query.CountAsync();
            var products = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var dtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                Name = p.Name,
                ImageUrl = "https://localhost:7049" + p.ProductImages?
                            .Where(i => i.IsMain)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault(),
                Price = p.Price,
                Quantity = p.Quantity,
                Rating = p.Rating,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                StoreId = p.StoreId,
                StoreLogo = p.Store.LogoUrl,
                StoreName = p.Store.StoreName
            }).ToList();

            return new ProductPerPage
            {
                Items = dtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ProductPerPage> GetProductsForStorePageAsync(
            int storeId,
            int? categoryId,
            int? brandId,
            string? pname,
            string? sname,
            decimal? minPrice,
            decimal? maxPrice,
            bool? latest,
            bool? highest,
            float? rating,
            int pageIndex,
            int pageSize)
        {
            var query = _context.Products.Include(c => c.Store).Include(c => c.Category).Include(pi => pi.ProductImages).Where(s => s.StoreId == storeId).AsQueryable();

            if (categoryId > 0)
                query = query.Where(p => p.CategoryId == categoryId);

            if (brandId > 0)
                query = query.Where(p => p.BrandId == brandId);

            if (!string.IsNullOrWhiteSpace(pname))
                query = query.Where(p => p.Name.Contains(pname));

            if (!string.IsNullOrWhiteSpace(sname))
                query = query.Where(p => p.Store.StoreName.Contains(sname));

            if (rating > 0)
                query = query.Where(p => p.Rating >= rating);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (highest == true)
                query = query.OrderByDescending(p => p.Price);
            else if (latest == true)
                query = query.OrderByDescending(p => p.CreatedAt);

            int totalCount = await query.CountAsync();

            var pagedProducts = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = pagedProducts.Select(p => new ProductDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                Name = p.Name,
                ImageUrl = p.ProductImages?
                .Where(i => i.IsMain)
                .Select(i => i.ImageUrl)
                .FirstOrDefault(),
                Price = p.Price,
                Quantity = p.Quantity,
                Rating = p.Rating,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                StoreId = p.StoreId,
                StoreLogo = p.Store.LogoUrl,
                StoreName = p.Store.StoreName
            }).ToList();

            return new ProductPerPage
            {
                Items = dtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private async Task<string> SaveImageToFolder(string base64String, string productName)
        {
            if (string.IsNullOrEmpty(base64String) || !base64String.Contains("base64,"))
                return base64String;

            try
            {
                // 1. Xử lý đường dẫn an toàn
                string webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "products");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // 2. Decode Base64
                var dataIndex = base64String.IndexOf("base64,") + 7;
                var cleanBase64 = base64String.Substring(dataIndex);
                var buffer = Convert.FromBase64String(cleanBase64);

                // 3. Tạo file name duy nhất
                var fileName = $"{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // 4. Ghi file
                await File.WriteAllBytesAsync(filePath, buffer);

                return $"/uploads/products/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi SaveImage: {ex.Message}");
                return string.Empty;
            }
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }
}

