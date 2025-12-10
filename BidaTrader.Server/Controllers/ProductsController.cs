using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IService<Product> _productService;

    public ProductsController(IService<Product> productService)
    {
        _productService = productService;
    }


    [HttpGet("all")]
    public async Task<ActionResult> GetAllProduct()
    {
        var products = await _productService.GetItemsAsync();
        var reponse = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            // Đã được Include từ Service
            CategoryName = p.Category?.Name ?? "N/A",
            ImageUrl = p.ProductImages.FirstOrDefault()?.ImageUrl ?? "img/default.png"
        }).ToList();
        return Ok(reponse);
    }

    [HttpGet]
    public async Task<ActionResult<ProductPerPage>> GetProducts([FromQuery] int? categoryId, [FromQuery] string? searchKey, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {

        var (products, totalCount) = await ((ProductService)_productService).GetProductsForPaginationAsync(
            categoryId, searchKey, pageIndex, pageSize);

        if (products == null)
        {
            return Ok(new ProductPerPage());
        }

        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            // Đã được Include từ Service
            CategoryName = p.Category?.Name ?? "N/A",
            ImageUrl = p.ProductImages.FirstOrDefault()?.ImageUrl ?? "img/default.png"
        }).ToList();

        // 3. Đóng gói vào PagedResponseDto
        var pagedResponse = new ProductPerPage
        {
            Items = dtos,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(pagedResponse); // ⬅️ TRẢ VỀ DTO PHÂN TRANG
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productService.GetItemByIdAsync(id);

        // 1. Sửa Status Code: 404 Not Found là chuẩn cho tài nguyên không tồn tại
        if (product == null) return NotFound("Không tìm thấy sản phẩm.");

        // 2. Mapping Entity -> DTO (Output)
        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "N/A",
            ImageUrl = product.ProductImages.FirstOrDefault()?.ImageUrl ?? "img/default.png"
        };

        // 3. Trả về DTO
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrduct([FromBody] ProductDto dto)
    {
        var item = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            StoreId = dto.StoreId
        };
        var created = await _productService.CreateItemAsync(item);
        if (!created) return BadRequest("Tạo mới sản phẩm thất bại.");
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto dto)
    {
        if (id != dto.Id) return BadRequest("ID không khớp.");
        var existingProduct = await _productService.GetItemByIdAsync(id);
        if (existingProduct == null) return NotFound("Không tìm thấy sản phẩm để cập nhật.");
        existingProduct.Name = dto.Name;
        existingProduct.Description = dto.Description;
        existingProduct.Price = dto.Price;
        existingProduct.CategoryId = dto.CategoryId;
        existingProduct.StoreId = dto.StoreId;
        existingProduct.UpdatedAt = DateTime.UtcNow;
        var updated = await _productService.UpdateItemAsync(existingProduct);
        if (!updated) return BadRequest("Cập nhật sản phẩm thất bại");
        return NoContent();
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var item = await _productService.GetItemByIdAsync(id);
        if (item == null) return NotFound("Không tìm thấy sản phẩm để xóa.");
        var deleted = await _productService.DeleteItemAsync(id);
        if (!deleted) return BadRequest("Xóa sản phẩm thất bại");
        return NoContent();
    }
}