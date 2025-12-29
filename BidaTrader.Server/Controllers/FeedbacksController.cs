using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidaTrader.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;

        public FeedbacksController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByProductAsync(productId);
            return Ok(feedbacks);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackDto dto)
        {
            var usedId = GetCurrentUserId();

            var result = await _feedbackService.CreateFeedbackAsync(usedId, dto);

            if (result == "SUCCESS")
                return Ok(new { message = "Đánh giá thành công!" });

            return BadRequest(new { message = result });
        }

        [HttpPost("reply")]
        public async Task<IActionResult> Reply([FromBody] ReplyFeedbackDto dto)
        {
            var usedId = GetCurrentUserId();

            var success = await _feedbackService.ReplyFeedbackAsync(usedId, dto);

            if (!success)
                return BadRequest("Không thể trả lời. Có thể bạn không phải chủ sở hữu sản phẩm này.");

            return Ok(new { message = "Đã gửi phản hồi." });
        }
  

            private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
            {
                return userId;
            }
            throw new Exception("User not found in token");
        }
    }
}