using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IService<Category> _categoryService;

        public CategoriesController(IService<Category> service) => _categoryService = service;

        [HttpGet]
        [Authorize(Policy = "CATEGORY.VIEW")]
        public async Task<ActionResult<CategoryPerPage>> GetCategoriesWithPagination([FromQuery] string? search, [FromQuery] int pageIndex =1, [FromQuery] int pageSize =10)
        {
            var (items, total) = await ((CategoryService)_categoryService).GetItemsPerPage(search, pageIndex, pageSize);
            
            var itemsDto = items.Select(item => new CategoryDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
            }).ToList();

            var response = new CategoryPerPage
            {
                Categories = itemsDto,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = total
            };

            return Ok(response);
        }

        [HttpGet("all")]
        public async Task<ActionResult<CategoryDto>> GetCategories()
        {
            var categories = await _categoryService.GetItemsAsync();
            var dtos = categories.Select(d => new CategoryDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
            });
            return Ok(dtos);
        }
        [HttpGet("{id:int}")]
        [Authorize(Policy = "CATEGORY.VIEW")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetItemByIdAsync(id);
            if (category == null) return NotFound();

            var response = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = "CATEGORY.CREATE")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto dto)
        {
            var item = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _categoryService.CreateItemAsync(item);
            if (!created) return StatusCode(500, "Tạo mới thất bại.");
            return NoContent();
        }
        
        [HttpPut("{id}")]
        [Authorize(Policy = "CATEGORY.UPDATE")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto dto)
        {
            if (id != dto.Id) return BadRequest("ID không khớp.");
            var existingCategory = await _categoryService.GetItemByIdAsync(id);
            if (existingCategory == null) return NotFound();
            if (!string.IsNullOrEmpty(dto.Name))
            {
                existingCategory.Name = dto.Name;
            }
            if (!string.IsNullOrEmpty(dto.Description))
            {
                existingCategory.Description = dto.Description;
            }
            existingCategory.UpdatedAt = DateTime.UtcNow;
            var updated = await _categoryService.UpdateItemAsync(existingCategory);
            if (!updated) return StatusCode(500, "Cập nhật thất bại.");
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "CATEGORY.DELETE")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var existingCategory = await _categoryService.GetItemByIdAsync(id);
            if (existingCategory == null) return NotFound();
            var deleted = await _categoryService.DeleteItemAsync(id);
            if (!deleted) return StatusCode(500, "Xóa thất bại.");
            return NoContent();
        }
    }
}