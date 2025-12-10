using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BidaTraderShared.Data.Models;
using BidaTraderShared.Data.Services;
using BidaTrader.Server.Services;
using BidaTraderShared.Data.DTOs;

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

        [HttpGet]
        public async Task<ActionResult> GetAccounts([FromQuery] string? username, [FromQuery] string? role , [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (accounts, totalItems) = await ((AccountService)_accountService).GetAccountWithPagination(username, role, pageIndex, pageSize);
            
            var dtos = accounts.Select( p => new AccountDto
            {
                Id = p.Id,
                UserName = p.UserName,
                Password = p.PasswordHash,
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
        
        [HttpGet("{id}")]        
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
                UserName = account.UserName,
                Password = account.PasswordHash,
                Role = account.Role,
                IsActive = account.IsActive
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
            account.PasswordHash = accountDto.Password;
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
