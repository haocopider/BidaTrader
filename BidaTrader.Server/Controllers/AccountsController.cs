using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BidaTrader.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountsController(AccountService service)
        {
            _accountService = service;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
                return userId;
            throw new UnauthorizedAccessException("User not found in token");
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ProfileDto>> GetMyProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var accountDto = await _accountService.GetAccountDetailAsync(userId);
                if (accountDto == null) return NotFound("Tài khoản không tồn tại.");
                return Ok(accountDto);
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] ProfileDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _accountService.UpdateProfileAsync(userId, dto);
                if (!success) return BadRequest("Cập nhật thất bại.");
                return Ok(new { message = "Cập nhật thành công" });
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        [HttpGet]
        public async Task<ActionResult<AccountPerPage>> GetAccounts([FromQuery] string? username, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAccountsPaginationAsync(username, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccountDto>> GetAccountById(int id)
        {
            var account = await _accountService.GetAccountDetailAsync(id);
            if (account == null) return NotFound();
            return Ok(account);
        }

        [HttpGet("uid/{uid}")]
        public async Task<ActionResult<AccountDto>> GetAccountByUID(string uid)
        {
            var account = await _accountService.GetAccountByUidAsync(uid);
            if (account == null) return NotFound();
            return Ok(account);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountDto model)
        {

            var existingAccount = await _accountService.GetItemByIdAsync(id);

            if (existingAccount == null)
            {
                return BadRequest("Không tìm thấy tài khoản.");
            }

            existingAccount.UserName = model.UserName;
            existingAccount.Email = model.Email;
            existingAccount.Phone = model.Phone;
            existingAccount.Passcode = model.Passcode;
            existingAccount.IsActive = model.IsActive;

            if (!string.IsNullOrEmpty(model.PasswordHash))
            {
                existingAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
            }

            try
            {
                var result = await _accountService.UpdateItemAsync(existingAccount);
                if (result)
                {
                    return Ok(new { message = "Cập nhật thành công." });
                }
                else
                {
                    return NotFound("Không tìm thấy tài khoản để cập nhật.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var success = await _accountService.DeleteAccountAsync(id);
            if (!success) return BadRequest("Xóa thất bại hoặc không tìm thấy tài khoản.");
            return Ok("Xóa thành công");
        }

        [HttpGet("has-passcode")]
        [Authorize]
        public async Task<IActionResult> HasPasscode()
        {
            try { return Ok(await _accountService.HasPasscodeAsync(GetCurrentUserId())); }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        [HttpPost("check-passcode")]
        [Authorize]
        public async Task<IActionResult> VerifyPasscode([FromBody] string passcode)
        {
            try
            {
                var isValid = await _accountService.CheckPasscodeAsync(GetCurrentUserId(), passcode);
                if (!isValid) return BadRequest("Mã Passcode không chính xác.");
                return Ok(new { message = "Xác thực thành công." });
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        [HttpPost("change-password-secure")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordSecure([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var success = await _accountService.ChangePasswordSecureAsync(GetCurrentUserId(), dto.NewPassword, dto.CurrentPasscode);
                if (!success) return BadRequest("Đổi mật khẩu thất bại. Sai Passcode hoặc lỗi hệ thống.");
                return Ok(new { message = "Đổi mật khẩu thành công." });
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        [HttpPost("forgot-password-request")]
        public async Task<IActionResult> ForgotPasswordRequest([FromBody] ForgotPasswordDto dto)
        {
            var result = await _accountService.SendForgotPasswordOtpAsync(dto.Email);
            if (result == "EMAIL_NOT_FOUND") return BadRequest("Email không tồn tại.");
            if (result.StartsWith("MAIL_ERROR")) return StatusCode(500, "Lỗi gửi mail.");
            return Ok(new { message = "OTP đã được gửi." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var isValid = await _accountService.VerifyOtpAsync(dto.Email, dto.Otp);
            if (!isValid) return BadRequest("OTP sai hoặc hết hạn.");
            return Ok(new { message = "OTP hợp lệ." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _accountService.ResetPasswordAsync(dto.Email, dto.Otp, dto.NewPassword);
            if (result == "SUCCESS") return Ok(new { message = "Đổi mật khẩu thành công." });
            return BadRequest("Lỗi: " + result);
        }
    }
}