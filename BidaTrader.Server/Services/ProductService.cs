using BidaTraderShared.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    // Kế thừa ServerGenericService để tận dụng code cũ, nhưng implement IService<Product>
    public class ProductService : ServerGenericService<Product>
    {
        public ProductService(AppDbContext context) : base(context) { }

        // GHI ĐÈ (OVERRIDE) hàm Create để thêm thời gian tạo
        public override async Task<bool> CreateItemAsync(Product product)
        {
            product.CreatedAt = DateTime.Now;
            product.IsActive = true;
            return await base.CreateItemAsync(product);
        }

        // GHI ĐÈ (OVERRIDE) hàm Update để xử lý Business Logic & Audit
        public override async Task<bool> UpdateItemAsync(Product productInput)
        {
            // 1. Lấy dữ liệu cũ từ DB
            var existingProduct = await _dbSet.FindAsync(productInput.Id);
            if (existingProduct == null) return false;

            // 2. Chỉ cập nhật các trường cho phép (Mapping an toàn)
            existingProduct.Name = productInput.Name;
            existingProduct.Description = productInput.Description;
            existingProduct.Price = productInput.Price;
            existingProduct.CategoryId = productInput.CategoryId;

            // 3. Logic Audit: Cập nhật thời gian sửa
            existingProduct.UpdatedAt = DateTime.Now;

            // 4. Lưu (EF Core tự nhận biết thay đổi)
            return await _context.SaveChangesAsync() > 0;
        }

        // GHI ĐÈ hàm GetItems để Include Category (Eager Loading)
        public override async Task<List<Product>?> GetItemsAsync(string? queryString = null)
        {
            // Query cơ bản có Include Category
            var products = _dbSet.Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Store)
                .AsQueryable();

            // Xử lý lọc đơn giản nếu cần thiết tại Service
            return await products.ToListAsync();
        }
    
        public async Task ToBin(List<Product> products)
        {
            foreach (var product in products)
            {
                var existingProduct = await _dbSet.FindAsync(product.Id);
                if (existingProduct != null)
                {
                    existingProduct.IsActive = false;
                    existingProduct.UpdatedAt = DateTime.Now;
                } 
            }
        }

        public async Task<List<Product>?> GetFilteredProductsAsync(int? categoryId, string? searchKey)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchKey))
            {
                query = query.Where(p => p.Name.ToLower().Contains(searchKey));
            }

            return await query.ToListAsync();
        }

        public async Task<(List<Product> Products, int TotalCount)> GetProductsForPaginationAsync(
    int? categoryId,
    string? searchKey,
    int pageIndex,
    int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Category) // Phải Include Category để mapping DTO
                .AsQueryable();

            // Lọc theo Category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Lọc theo Search Key
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                string searchLower = searchKey!.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchLower));
            }

            // 1. Lấy tổng số lượng (trước khi phân trang)
            int totalCount = await query.CountAsync();

            // 2. Phân trang (Skip/Take)
            var pagedProducts = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (pagedProducts, totalCount);
        }
    }
}