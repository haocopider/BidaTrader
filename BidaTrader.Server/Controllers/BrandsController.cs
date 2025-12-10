using BidaTrader.Server.Services;
using BidaTraderShared.Data.DTOs;
using BidaTraderShared.Data.Models;
using BidaTraderShared.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IService<Brand> _brandService;

        public BrandsController(IService<Brand> brandService) => _brandService = brandService;

        [HttpGet("header")]
        public async Task<ActionResult<List<BrandDto>>> GetBrandHeaders()
        {
            var brands = await _brandService.GetItemsAsync();
            var brandDtos = brands.Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
            }).ToList();
            return Ok(brandDtos);
        }

        [HttpGet]
        public async Task<ActionResult<BrandPerPage>> GetBrands(string? search, int pageIndex = 1, int pageSize = 10)
        {
            var (items, total) = await ((BrandService)_brandService).GetItemsPerPage(search, pageIndex, pageSize);

            var itemDtos = items.Select(p => new BrandDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                OwnerStoreId = p.OwnerStoreId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            var responese = new BrandPerPage
            {
                Items = itemDtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = total
            };
            return Ok(responese);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BrandDto>> GetBrand(int id)
        {
            var brand = await _brandService.GetItemByIdAsync(id);
            if (brand == null) return NotFound();
            var response = new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                OwnerStoreId = brand.OwnerStoreId,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromBody] BrandDto dto)
        {
            var item = new Brand
            {
                Name = dto.Name,
                Description = dto.Description,
                OwnerStoreId = dto.OwnerStoreId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _brandService.CreateItemAsync(item);
            if (!created) return BadRequest("Tạo mới thất bại");
            return Ok("Tạo mới thành công");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBrand(int id, [FromBody] BrandDto dto)
        {
            var item = await _brandService.GetItemByIdAsync(id);
            if (item == null) return NotFound("Không tìm thấy thương hiệu.");
            item.Name = dto.Name;
            item.Description = dto.Description;
            item.OwnerStoreId = dto.OwnerStoreId;
            item.UpdatedAt = DateTime.UtcNow;
            var updated = await _brandService.UpdateItemAsync(item);
            if (!updated) return BadRequest("Cập nhật thất bại");
            return Ok("Cập nhật thành công");
        }

    }
}
