using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class PostService : ServerService<Post>
    {
        public PostService(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> CreatePostAsync(int accountId,  CreatePostDto dto)
        {
            var post = new Post
            {
                AccountId = accountId,
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = dto.ThumbnailUrl
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddCommentAsync(int accountId, CreateCommentDto dto)
        {
            var comment = new Comment
            {
                PostId = dto.PostId,
                AccountId = accountId,
                Content = dto.Content
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<Post> Posts, int totalItems)> GetPostWithPagnination(string? title, string? author, bool? isActive = true, int pageIndex = 1, int pageSize = 20)
        {
            var query = _context.Posts.Include(p => p.Account).AsQueryable();

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
