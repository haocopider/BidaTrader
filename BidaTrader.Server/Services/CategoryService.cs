using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class CategoryService : ServerService<Category>
    {

        public CategoryService(AppDbContext context) : base(context){}

        public async Task<(List<Category> Categories, int TotalCount)> GetItemsPerPage(string? search, int pageIndex = 1, int pageSize=10 )
        {
            var query = _context.Categories.AsQueryable();

            int totalItems = await query.CountAsync();

            var pageItems = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (pageItems, totalItems);
        }
    }
}
