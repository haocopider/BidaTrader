using BidaTrader.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidaTrader.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("my-orders")]
        [Authorize(Policy = "ORDER.VIEW")]
        public async Task<IActionResult> GetMyOrders([FromQuery] string? status)
        {
            var userId = GetCurrentUserId();
            var result = await _orderService.GetMyOrders(userId, status);

            return Ok(result);
        }

        [HttpGet("{orderId}")]
        [Authorize(Policy = "ORDER.VIEW")]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            var userId = GetCurrentUserId();

            var dto = await _orderService.GetOrderDetail(orderId, userId);

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        [HttpGet("store-orders")]
        [Authorize(Policy = "ORDER.VIEW")]
        [Authorize(Roles ="STORE")]
        public async Task<IActionResult> GetStoreOrders()
        {
            var userId = GetCurrentUserId();
            var orders = await _orderService.GetStoreOrders(userId);

            return Ok(orders);
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Policy = "ORDER.UPDATE")]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            [FromBody] string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                return BadRequest("Status không hợp lệ");

            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            await _orderService.UpdateOrderStatus(userId, orderId, newStatus, isAdmin);

            return Ok(new { message = "Cập nhật trạng thái thành công" });
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