using Azure.Core;
using BidaTrader.Server.Helpers;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
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
    
        public async Task<Account> GetAccountByUIDAsync(string uid)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Uid == uid);
        }

        public async Task<List<string>> GetPermissionAsync(int accountId)
        {
            return await _context.AccountRoles
                .Where(ar => ar.AccountId == accountId)
                .SelectMany(ar => ar.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();
        }
    }
}
