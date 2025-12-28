using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class StoreService : ServerService<Store>
    {
        public StoreService(AppDbContext context) : base(context)
        {
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
    }
}
