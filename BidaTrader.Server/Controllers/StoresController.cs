using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace BidaTrader.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly IService<Store> _storeService;
        public StoresController(IService<Store> service)
        {
            _storeService = service;
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

        [HttpGet("mystore")]
        public async Task<ActionResult<StoreDetailDto>> GetMyStore(string accountId)
        {
            var usedId = GetCurrentUserId();
            var response = ((StoreService)_storeService).GetMyStore(usedId);
            if (response == null) BadRequest("Chưa mở cửa hàng");
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult> GetStores([FromQuery] string? seachKey, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (stores, totalItems) = await ((StoreService)_storeService).GetStoresWithPagination(seachKey, pageIndex, pageSize);
            var dtos = stores.Select(s => new StoreDto
            {
                Id = s.Id,
                AccountId = s.AccountId,
                StoreName = s.StoreName,
                LogoUrl = s.LogoUrl,
                Phone = s.Phone,
                Address = s.Address,
                Description = s.Description,
                SocialLinks = s.SocialLinks,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList();
            var response = new StorePerPage
            { 
                Stores = dtos,
                TotalCount = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetStoreById(int id)
        {
            var store = await _storeService.GetItemByIdAsync(id);
            if (store == null)
            {
                return NotFound();
            }
            var dto = new 
            {
                store.Id,
                store.AccountId,
                store.StoreName,
                store.Phone,
                store.Address,
                store.LogoUrl,
                store.Description,
                store.SocialLinks,
                store.IsActive,
                store.CreatedAt,
                store.UpdatedAt
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] StoreDto store)
        {
            if (store == null) BadRequest();
            var newStore = new Store
            {
                AccountId = store.AccountId,
                StoreName = store.StoreName,
                Phone = store.Phone,
                Address = store.Address,
                Description = store.Description,
                SocialLinks = store.SocialLinks,
                IsActive = store.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var created = await _storeService.CreateItemAsync(newStore);
            if (created == null)
            {
                return BadRequest("Could not create store");
            }
            return Ok("Store created successfully");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStore(int id, [FromBody] StoreDto store)
        {
            var existingStore = await _storeService.GetItemByIdAsync(id);
            if (existingStore == null)
            {
                return NotFound();
            }
            existingStore.StoreName = store.StoreName;
            existingStore.Phone = store.Phone;
            existingStore.Address = store.Address;
            existingStore.Description = store.Description;
            existingStore.SocialLinks = store.SocialLinks;
            existingStore.IsActive = store.IsActive;
            existingStore.UpdatedAt = DateTime.UtcNow;
            var updated = await _storeService.UpdateItemAsync(existingStore);
            if (!updated)
            {
                return BadRequest("Could not update store");
            }
            return Ok("Store updated successfully");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var existingStore = await _storeService.GetItemByIdAsync(id);
            if (existingStore == null)
            {
                return NotFound();
            }
            var deleted = await _storeService.DeleteItemAsync(id);
            if (!deleted)
            {
                return BadRequest("Could not delete store");
            }
            return Ok("Store deleted successfully");
        }
    }
}
