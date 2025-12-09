using BidaTraderShared.Data.DTOs;
using BidaTraderShared.Data.Models;
using BidaTraderShared.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class AccountService : ServerService<Account>
    {
        public AccountService(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<Account> Accounts, int totalItems)> GetAccountWithPagination(string? search, int pageIndex=1, int pageSize =10)
        {
            var query = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.UserName.Contains(search));
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
