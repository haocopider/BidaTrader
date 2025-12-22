using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidaTrader.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CartService _service;

        public CartController(CartService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<CartGroupDto>>> GetMyCart()
        {
            var userId = GetCurrentUserId();

            var groupedCart = await _service.MyCart(userId);

            return Ok(groupedCart);
        }

        // 2. Thêm sản phẩm vào giỏ
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto request)
        {
            var userId = GetCurrentUserId();

            var item = new Cart
            {
                AccountId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow,
            };

            await _service.AddToCart(userId, item);

            return Ok(new { message = "Đã thêm vào giỏ hàng" });
        }

        // 3. Cập nhật số lượng
        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] AddToCartDto request)
        {
            var userId = GetCurrentUserId();

            var item = new Cart
            {
                AccountId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            await _service.UpdateQuantity(userId, item);

            return Ok(new { message = "Cập nhật số lượng thành công" });
        }

        // 4. Xóa sản phẩm khỏi giỏ
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = GetCurrentUserId();

            var deleted = ((CartService)_service).DeleteProductInCart(userId, productId);

            return Ok();
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
        {
            var userId = GetCurrentUserId();

            await _service.Checkout(userId, request);

            return Ok(new { message = "Đặt hàng thành công" });
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