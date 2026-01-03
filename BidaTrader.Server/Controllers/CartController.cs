using BidaTrader.Server.Helpers;
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
        private readonly VnPayService _vnpayService;

        public CartController(CartService service, VnPayService vnpayService)
        {
            _service = service;
            _vnpayService = vnpayService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CartGroupDto>>> GetMyCart()
        {
            var userId = GetCurrentUserId();

            var groupedCart = await _service.MyCart(userId);

            return Ok(groupedCart);
        }

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

            var (orderIds, totalAmount) = await _service.Checkout(userId, request);

            string combinedOrderId = string.Join("_", orderIds);

            return Ok(new
            {
                orderId = combinedOrderId,
                totalAmount = (long)totalAmount,
                message = "Đặt hàng thành công"
            });
        }
        [HttpPost("create-vnpay-url")]
        public IActionResult CreateVnPayUrl([FromBody] PaymentRequestDto model)
        {
            try
            {
                var url = _vnpayService.CreatePaymentUrl(HttpContext, model);

                return Ok(new { PayUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi tạo link thanh toán: " + ex.Message);
            }
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
             var (checkSignature, vnp_TxnRef, vnp_ResponseCode) = _vnpayService.PayReturn(Request.Query);

            if (checkSignature)
            {
                if (vnp_ResponseCode == "00")
                {
                    return Redirect($"/payment-result?success=true&orderId={vnp_TxnRef}");
                }
                else
                {
                    return Redirect($"/payment-result?success=false&orderId={vnp_TxnRef}&errorCode={vnp_ResponseCode}");
                }
            }
            else
            {
                return BadRequest("Sai chữ ký bảo mật!");
            }
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