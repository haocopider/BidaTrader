using BidaTrader.Server.Helpers;
using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BidaTrader.Server.Controllers
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

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<AccountDto>> GetMyProfile()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var account = await _accountService.GetItemByIdAsync(userId);
            if (account == null) return NotFound("Không tìm thấy tài khoản.");

            return Ok(new AccountDto
            {
                Id = account.Id,
                UID = account.Uid,
                UserName = account.UserName,
                Email = account.Email,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Phone = account.Phone,
                Address = account.Address,
                AvatarUrl = account.AvatarUrl,
                IsActive = account.IsActive,
                DateOfBirth = account.DateOfBirth,
            });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] AccountDto dto)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var account = await _accountService.GetItemByIdAsync(userId);
            if (account == null) return NotFound();
            account.LastName = dto.LastName;
            account.FirstName = dto.FirstName;
            account.Email = dto.Email;
            account.Phone = dto.Phone;
            account.Address = dto.Address;

            if (!string.IsNullOrEmpty(dto.AvatarUrl) && dto.AvatarUrl.StartsWith("data:image"))
            {
                // Logic lưu file ảnh (Giả sử bạn có hàm helper lưu file)
                // account.AvatarUrl = await _fileService.SaveBase64Image(dto.AvatarUrl);

                // Tạm thời nếu chưa có logic lưu file, cẩn thận kẻo lỗi DB vì chuỗi quá dài
                // account.AvatarUrl = dto.AvatarUrl; 
            }

            var updated = await _accountService.UpdateItemAsync(account);
            if (!updated) return StatusCode(500, "Cập nhật thông tin thất bại.");

            return Ok(new { message = "Cập nhật thành công" });
        }
        [HttpGet]
        public async Task<ActionResult> GetAccounts([FromQuery] string? username, [FromQuery] string? role , [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (accounts, totalItems) = await ((AccountService)_accountService).GetAccountWithPagination(username, role, pageIndex, pageSize);
            
            var dtos = accounts.Select( p => new AccountDto
            {
                Id = p.Id,
                UID = p.Uid,
                UserName = p.UserName,
                PasswordHash = p.PasswordHash,
                Role = p.Role,
                IsActive = p.IsActive
            }).ToList();
            
            var response = new AccountPerPage
            {
                Accounts = dtos,
                TotalCount = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetAccountById(int id)
        {
            var account = await _accountService.GetItemByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            var dto = new AccountDto
            {
                Id = account.Id,
                UID = account.Uid,
                UserName = account.UserName,
                PasswordHash = account.PasswordHash,
                Role = account.Role,
                IsActive = account.IsActive,
                AvatarUrl = account.AvatarUrl,
                DateOfBirth = account.DateOfBirth,
                Address = account.Address,
                Email = account.Email,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Phone = account.Phone
            };
            return Ok(dto);
        }

        [HttpGet("uid/{uid}")]        
        public async Task<ActionResult> GetAccountByUID(string uid)
        {
            var account = await ((AccountService)_accountService).GetAccountByUIDAsync(uid);
            if (account == null)
            {
                return NotFound();
            }
            var dto = new AccountDto
            {
                Id = account.Id,
                UID = account.Uid,
                UserName = account.UserName,
                PasswordHash = account.PasswordHash,
                Role = account.Role,
                IsActive = account.IsActive,
                AvatarUrl = account.AvatarUrl,
                DateOfBirth = account.DateOfBirth,
                Address = account.Address,
                Email = account.Email,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Phone = account.Phone
            };
            return Ok(dto);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto accountDto)
        {
            var userId = GetCurrentUserId();

            var success = await _accountService.UpdateProfileAsync(userId, accountDto);

            if (!success)
            {
                return BadRequest("Cập nhật hồ sơ thất bại hoặc không tìm thấy tài khoản.");
            }

            return Ok(new { message = "Cập nhật thành công!" });
        }
        

        [HttpDelete("{id}")]    
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _accountService.GetItemByIdAsync(id);
            if (account == null)
            {
                return BadRequest("Không tìm thấy tài khoản");
            }
            var deleted = await _accountService.DeleteItemAsync(id);
            if (!deleted)
            {
                return StatusCode(500, "Xóa tài khoản thất bại.");
            }
            return Ok("Xóa thành công");
        }



        [HttpGet("has-passcode")]
        [Authorize]
        public async Task<IActionResult> HasPasscode()
        {
            var userId = GetCurrentUserId();
            var result = await _accountService.HasPasscodeAsync(userId);
            return Ok(result);
        }

        [HttpPost("check-passcode")]
        [Authorize]
        public async Task<IActionResult> VerifyPasscode([FromBody] string passcode)
        {
            var userId = GetCurrentUserId();
            var isValid = await _accountService.CheckPasscodeAsync(userId, passcode);

            if (!isValid) return BadRequest("Mã Passcode không chính xác.");

            return Ok(new { message = "Xác thực thành công." });
        }

        [HttpPost("change-password-secure")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordSecure([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();

            var success = await _accountService.ChangePasswordSecureAsync(userId, dto.NewPassword, dto.CurrentPasscode);

            if (!success)
            {
                return BadRequest("Đổi mật khẩu thất bại. Vui lòng kiểm tra lại Passcode.");
            }

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }


        [HttpPost("forgot-password-request")]
        public async Task<IActionResult> ForgotPasswordRequest([FromBody] ForgotPasswordDto dto)
        {
            var result = await _accountService.SendForgotPasswordOtpAsync(dto.Email);

            if (result == "EMAIL_NOT_FOUND")
                return BadRequest("Email không tồn tại trong hệ thống.");

            if (result.StartsWith("MAIL_ERROR"))
                return StatusCode(500, "Lỗi gửi email. Vui lòng thử lại sau.");

            return Ok(new { message = "Mã OTP đã được gửi đến email của bạn." });
        }

        // BƯỚC 2: Kiểm tra OTP (Để chuyển màn hình)
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var isValid = await _accountService.VerifyOtpAsync(dto.Email, dto.Otp);

            if (!isValid)
                return BadRequest("Mã OTP không đúng hoặc đã hết hạn.");

            return Ok(new { message = "OTP hợp lệ." });
        }

        // BƯỚC 3: Đổi mật khẩu
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _accountService.ResetPasswordAsync(dto.Email, dto.Otp, dto.NewPassword);

            if (result == "SUCCESS")
                return Ok(new { message = "Đổi mật khẩu thành công." });

            if (result == "OTP_INVALID_OR_EXPIRED")
                return BadRequest("Phiên giao dịch hết hạn, vui lòng yêu cầu lại OTP.");

            return BadRequest("Có lỗi xảy ra.");
        }


        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            return Ok(await _accountService.GetAllPermissionsAsync());
        }

        [HttpGet("with-permissions")]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _accountService.GetAllRolesWithPermissionsAsync());
        }

        [HttpPut("update-permissions")]
        public async Task<IActionResult> UpdatePermissions([FromBody] UpdateRolePermissionsDto dto)
        {
            var result = await _accountService.UpdateRolePermissionsAsync(dto);
            if (!result) return BadRequest("Cập nhật thất bại");
            return Ok("Cập nhật quyền thành công");
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
