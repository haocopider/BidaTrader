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

        public async Task<(List<Account> Accounts, int totalItems)> GetAccountWithPagination(string? username, string? role, int pageIndex=1, int pageSize = 10)
        {
            var query = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(a => a.UserName.Contains(username));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(a => a.Role.Contains(role));
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
