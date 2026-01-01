using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidaTrader.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RolesController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleWithPermissionsDto>>> GetAll()
        {
            return Ok(await _roleService.GetAllRolesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleDto dto)
        {
            var result = await _roleService.CreateRoleAsync(dto);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        [HttpPut] // Thêm API Update Role
        public async Task<IActionResult> Update([FromBody] RoleDto dto)
        {
            var result = await _roleService.UpdateRoleAsync(dto);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok();
        }

        [HttpPut("permissions")]
        public async Task<IActionResult> UpdatePermissions([FromBody] UpdateRolePermissionsDto dto)
        {
            var success = await _roleService.UpdateRolePermissionsAsync(dto);
            if (!success) return StatusCode(500, "Lỗi cập nhật quyền.");
            return Ok();
        }
    }
}