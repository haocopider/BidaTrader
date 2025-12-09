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

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult> GetAccounts([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 5)
        {
            var (accounts, totalItems) = await ((AccountService)_accountService).GetAccountWithPagination(search, pageIndex, pageSize);
            
            var dtos = accounts.Select( p => new AccountDto
            {
                Id = p.Id,
                UserName = p.UserName,
                Password = p.PasswordHash,
                Role = p.Role,
                IsActive = p.IsActive
            });
            
            var response = new
            {
                Data = dtos,
                TotalItems = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Ok(response);
        }
    }
}
