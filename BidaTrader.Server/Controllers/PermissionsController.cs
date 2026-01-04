using BidaTrader.Server.Services;
using BidaTrader.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidaTrader.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly PermissionService _permService;

        public PermissionsController(PermissionService permService)
        {
            _permService = permService;
        }

        [HttpGet]
        [Authorize(Policy = "PERM.VIEW")]
        public async Task<ActionResult<List<Permission>>> GetAll()
        {
            return Ok(await _permService.GetAllAsync());
        }

        [HttpPost]
        [Authorize(Policy = "PERM.CREATE")]
        public async Task<IActionResult> Create([FromBody] Permission dto)
        {
            var result = await _permService.CreateAsync(dto);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        [HttpPut]
        [Authorize(Policy = "PERM.UPDATE")]
        public async Task<IActionResult> Update([FromBody] Permission dto)
        {
            var result = await _permService.UpdateAsync(dto);
            if (!result.IsSuccess) return NotFound(result.Message);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "PERM.DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _permService.DeleteAsync(id);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok();
        }
    }
}