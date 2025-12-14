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
