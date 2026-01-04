using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BidaTrader.Server.Services
{
    public class StoreService : ServerService<Store>
    {
        private readonly IWebHostEnvironment _env;
        public StoreService(AppDbContext context , IWebHostEnvironment env) : base(context)
        {
            _env = env;
        }

        #region Thống kê doanh thu

        public async Task<StoreDashboardSummaryDto> GetDashboardSummaryAsync(int userId)
        {
            // 1. Lấy thông tin Shop
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.AccountId == userId);
            if (store == null) return null;

            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);
            var endOfLastMonth = startOfThisMonth.AddDays(-1);

            // 2. Query cơ bản (Chỉ lấy đơn hoàn tất)
            var completedOrders = _context.Orders.Where(o => o.StoreId == store.Id && o.Status == "Completed");

            // --- TÍNH TOÁN THÁNG NAY ---
            var thisMonthData = await completedOrders
                .Where(o => o.OrderDate >= startOfThisMonth)
                .GroupBy(o => 1) // Group dummy để tính sum
                .Select(g => new {
                    Revenue = g.Sum(x => x.TotalAmount),
                    OrderCount = g.Count(),
                    // Tính tổng số lượng sản phẩm bán ra (cần join OrderDetail)
                    ProductsSold = g.SelectMany(x => x.OrderDetails).Sum(d => d.Quantity)
                }).FirstOrDefaultAsync();

            // --- TÍNH TOÁN THÁNG TRƯỚC (Để so sánh) ---
            var lastMonthData = await completedOrders
                .Where(o => o.OrderDate >= startOfLastMonth && o.OrderDate <= endOfLastMonth)
                .GroupBy(o => 1)
                .Select(g => new {
                    Revenue = g.Sum(x => x.TotalAmount),
                    OrderCount = g.Count()
                }).FirstOrDefaultAsync();

            // --- TÍNH CÁC CHỈ SỐ KHÁC ---
            var activeProducts = await _context.Products.CountAsync(p => p.StoreId == store.Id && p.IsActive == true);
            // Giả sử có bảng Follow, nếu chưa có thì để 0
            var followers = 0; // await _context.StoreFollows.CountAsync(f => f.StoreId == store.Id);

            // --- LẤY DỮ LIỆU BIỂU ĐỒ (12 tháng năm nay) ---
            var chartData = await completedOrders
                .Where(o => o.OrderDate.Year == now.Year)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new MonthlyRevenueStatDto
                {
                    Month = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                }).ToListAsync();

            // Fill đầy đủ 12 tháng (tránh tháng nào không bán được bị thiếu)
            var fullChartData = new List<MonthlyRevenueStatDto>();
            for (int i = 1; i <= 12; i++)
            {
                var stat = chartData.FirstOrDefault(x => x.Month == i)
                           ?? new MonthlyRevenueStatDto { Month = i, Revenue = 0, OrderCount = 0 };
                fullChartData.Add(stat);
            }

            // 3. Đóng gói DTO trả về
            var summary = new StoreDashboardSummaryDto
            {
                StoreId = store.Id,
                StoreName = store.StoreName,
                AvatarUrl = store.LogoUrl,
                Address = store.Address ?? "Chưa cập nhật",
                JoinDate = store.CreatedAt ?? DateTime.Now,

                CurrentMonthRevenue = thisMonthData?.Revenue ?? 0,
                CurrentMonthOrders = thisMonthData?.OrderCount ?? 0,
                TotalProductsSold = thisMonthData?.ProductsSold ?? 0,
                TotalActiveProducts = activeProducts,
                TotalFollowers = followers,
                RevenueChartData = fullChartData
            };

            // Tính % tăng trưởng
            if (lastMonthData != null && lastMonthData.Revenue > 0)
            {
                summary.RevenueGrowth = (double)((summary.CurrentMonthRevenue - lastMonthData.Revenue) / lastMonthData.Revenue) * 100;
                summary.OrderGrowth = (double)((summary.CurrentMonthOrders - lastMonthData.OrderCount) / (double)lastMonthData.OrderCount) * 100;
            }
            else if (summary.CurrentMonthRevenue > 0)
            {
                summary.RevenueGrowth = 100;
            }

            return summary;
        }

        public async Task<List<MonthlyRevenueStatDto>> GetMonthlyRevenueAsync(int storeId, int year)
        {
            // Chỉ lấy các đơn hàng đã hoàn thành của cửa hàng đó trong năm được chọn
            var query = _context.Orders
                .Where(o => o.StoreId == storeId &&
                            o.Status == "Completed" && // Thay đổi trạng thái phù hợp với DB của bạn
                            o.OrderDate.Year == year);

            // Gom nhóm theo tháng
            var stats = await query
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new MonthlyRevenueStatDto
                {
                    Month = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            // Lấp đầy các tháng không có doanh thu (để biểu đồ không bị đứt đoạn)
            var fullStats = new List<MonthlyRevenueStatDto>();
            for (int i = 1; i <= 12; i++)
            {
                var existing = stats.FirstOrDefault(s => s.Month == i);
                fullStats.Add(existing ?? new MonthlyRevenueStatDto { Month = i, Revenue = 0, OrderCount = 0 });
            }

            return fullStats;
        }

        public async Task<List<YearlyRevenueStatDto>> GetYearlyRevenueAsync(int storeId)
        {
            var query = _context.Orders
                .Where(o => o.StoreId == storeId &&
                            o.Status == "Completed");

            var stats = await query
                .GroupBy(o => o.OrderDate.Year)
                .Select(g => new YearlyRevenueStatDto
                {
                    Year = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.Year) // Năm mới nhất lên đầu
                .ToListAsync();

            return stats;
        }

        public async Task<List<int>> GetAvailableYearsAsync(int storeId)
        {
            return await _context.Orders
               .Where(o => o.StoreId == storeId && o.Status == "Completed")
               .Select(o => o.OrderDate.Year)
               .Distinct()
               .OrderByDescending(y => y)
               .ToListAsync();
        }
        #endregion

        private async Task<string> SaveStoreLogoAsync(string base64String, string storeName)
        {
            if (string.IsNullOrEmpty(base64String) || !base64String.StartsWith("data:image"))
                return base64String;

            try
            {
                string webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "stores");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Làm sạch tên file
                string safeName = string.Join("_", storeName.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"logo_{safeName}_{DateTime.Now.Ticks}.jpg";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Decode & Save
                var dataIndex = base64String.IndexOf("base64,") + 7;
                var buffer = Convert.FromBase64String(base64String.Substring(dataIndex));
                await File.WriteAllBytesAsync(filePath, buffer);

                return $"/uploads/stores/{fileName}";
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<string> RegisterStoreAsync(int userId, CreateStoreDto dto)
        {
            bool exists = await _context.Stores.AnyAsync(s => s.AccountId == userId);
            if (exists) return "Tài khoản này đã sở hữu một cửa hàng.";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string logoPath = await SaveStoreLogoAsync(dto.LogoUrl, dto.Name);

                var newStore = new Store
                {
                    AccountId = userId,
                    StoreName = dto.Name,
                    Description = dto.Description,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    LogoUrl = logoPath,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                };

                _context.Stores.Add(newStore);
                await _context.SaveChangesAsync();

                var account = await _context.Accounts.FindAsync(userId);
                if (account == null) throw new Exception("Không tìm thấy tài khoản.");

                account.StoreId = newStore.Id;
                _context.Accounts.Update(account);

                var storeRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "STORE");
                if (storeRole == null) throw new Exception("Hệ thống chưa cấu hình Role STORE.");

                var currentAccountRoles = await _context.AccountRoles
                    .Where(ar => ar.AccountId == userId)
                    .ToListAsync();

                if (currentAccountRoles.Any())
                {
                    _context.AccountRoles.RemoveRange(currentAccountRoles);
                }

                var newAccountRole = new AccountRole
                {
                    AccountId = userId,
                    RoleId = storeRole.Id
                };
                _context.AccountRoles.Add(newAccountRole);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "SUCCESS";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine($"Register Store Error: {ex}");

                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return "Lỗi chi tiết: " + innerException;
            }
        }

        public async Task<string> EditStoreAsync(int userId, CreateStoreDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return "Tên cửa hàng không được để trống.";

                var myStore = await _context.Stores
                    .FirstOrDefaultAsync(s => s.AccountId == userId);

                if (myStore == null)
                    return "Không tìm thấy cửa hàng của bạn.";

                if (!string.IsNullOrEmpty(dto.LogoUrl))
                {
                    myStore.LogoUrl = await SaveStoreLogoAsync(dto.LogoUrl, dto.Name);
                }

                myStore.StoreName = dto.Name;
                myStore.Description = dto.Description;
                myStore.Address = dto.Address;
                myStore.Phone = dto.Phone;
                myStore.UpdatedAt = DateTime.UtcNow;

                _context.Stores.Update(myStore);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "SUCCESS";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine($"Edit Store Error: {ex}");

                return "Lỗi chi tiết: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }


        public async Task<StoreDetailDto> GetMyStore(int accountId)
        {
            var myStore = await _context.Stores.Include(a => a.Accounts).FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (myStore == null)
            {
                return (new StoreDetailDto());
            }

            var dto = new StoreDetailDto
            {
                StoreName = myStore.StoreName,
                Address = myStore.Address ?? "Chưa cập nhật địa chỉ",
                AvatarUrl = myStore.LogoUrl,
                CoverUrl = myStore.LogoUrl,
                Phone= myStore.Phone ?? "Chưa cập nhật số điện thoại",
                Description = myStore.Description ?? "Chưa có mô tả về cửa hàng",
                TotalProducts = myStore.Products.Select(p => p.StoreId = myStore.Id).Count(),
                Rating = 5,
                Followers = 1000,
                JoinDate = myStore.CreatedAt ?? DateTime.Now,
                Id = myStore.Id                
            };

            return dto;
        }

        public async Task<(List<Store> Stores, int TotalCount)> GetStoresWithPagination(string? Sname, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.Stores.AsQueryable();
            if (!string.IsNullOrEmpty(Sname))
            {
                query = query.Where(s => s.StoreName.Contains(Sname));
            }
            int totalItems = await query.CountAsync();
            var pageItems = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (pageItems, totalItems);
        }

        public async Task<int> GetStoreIdAsync(int userId)
        {
            var storeId = await _context.Stores
                .Where(s => s.AccountId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            return storeId;
        }
    }
}
