using BidaTraderShared.Data.Models;
using BidaTraderShared.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class PostService : ServerService<Post>
    {
        public PostService(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<Post> Posts, int totalItems)> GetPostWithPagnination(string? title, string? author, bool? isActive = true, int pageIndex = 1, int pageSize = 20)
        {
            var query = _context.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(p => p.Title.Contains(title));
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(p => p.Account.UserName.Contains(author));
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
