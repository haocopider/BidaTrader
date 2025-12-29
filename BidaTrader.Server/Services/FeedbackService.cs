using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class FeedbackService : ServerService<Feedback>
    {
        public FeedbackService(AppDbContext context) : base(context)
        {
        }

        // --- 1. LẤY DANH SÁCH FEEDBACK ---
        public async Task<List<FeedbackDto>> GetFeedbacksByProductAsync(int productId)
        {
            return await _context.Feedbacks
                .Include(f => f.Account)
                .Include(f => f.FeedbackImages)
                .Where(f => f.ProductId == productId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackDto
                {
                    Id = f.Id,
                    CustomerName = f.Account.UserName ?? "Khách hàng", 
                    CustomerAvatar = f.Account.AvatarUrl,
                    Rating = f.Rating,
                    Content = f.Comment,
                    CreatedAt = f.CreatedAt ?? DateTime.Now,
                    Images = f.FeedbackImages.Select(i => i.ImageUrl).ToList()
                })
                .ToListAsync();
        }


        private async Task UpdateProductRating(int productId, int newRating)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            // Lấy danh sách rating cũ
            var existingRatings = await _context.Feedbacks
                .Where(f => f.ProductId == productId)
                .Select(f => f.Rating)
                .ToListAsync();

            existingRatings.Add(newRating);

            if (existingRatings.Any())
            {
                product.Rating = (int)Math.Round(existingRatings.Average());
            }
        }

        public async Task<string> CreateFeedbackAsync(int customerId, CreateFeedbackDto dto)
        {
            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.Id == dto.OrderDetailId && od.ProductId == dto.ProductId);

            if (orderDetail == null || orderDetail.Order.AccountId != customerId)
                return "Bạn chưa mua sản phẩm này.";

            // 3. Tạo Feedback
            var feedback = new Feedback
            {
                ProductId = dto.ProductId,
                AccountId = customerId,
                Rating = dto.Rating,
                Comment = dto.Content,
                FeedbackImages = dto.ImageUrls.Select(url => new FeedbackImage { ImageUrl = url }).ToList()
            };

            _context.Feedbacks.Add(feedback);

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product != null)
            {
                var ratings = await _context.Feedbacks.Where(f => f.ProductId == dto.ProductId).Select(f => f.Rating).ToListAsync();
                ratings.Add(dto.Rating);
                product.Rating = (int)Math.Round(ratings.Average());
            }

            await _context.SaveChangesAsync();
            return "SUCCESS";
        }

        public async Task<bool> ReplyFeedbackAsync(int storeId, ReplyFeedbackDto dto)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.Product)
                .FirstOrDefaultAsync(f => f.Id == dto.FeedbackId);

            if (feedback == null || feedback.Product.StoreId != storeId) return false;

            feedback.Comment = dto.ReplyContent;
            feedback.CreatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
