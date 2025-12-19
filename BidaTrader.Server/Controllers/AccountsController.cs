using BidaTrader.Server.Helpers;
using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IService<Account> _accountService;

        public AccountsController(IService<Account> service)
        {
            _accountService = service;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var account = await _accountService.GetItemByIdAsync(userId);
            if (account == null) return NotFound("Không tìm thấy tài khoản.");

            return Ok(new UserDto
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
        public async Task<IActionResult> UpdateMyProfile([FromBody] UserDto dto)
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, AccountDto accountDto)
        {
            if (id != accountDto.Id)
            {
                return BadRequest();
            }
            var account = await _accountService.GetItemByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            account.UserName = accountDto.UserName;
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountDto.PasswordHash);
            account.Role = accountDto.Role;
            account.IsActive = accountDto.IsActive ?? account.IsActive;
            var updated =   await _accountService.UpdateItemAsync(account);
            if (!updated)
            {
                return StatusCode(500, "Cập nhật thông tin thất bại.");
            }
            return NoContent();
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

    }
}
