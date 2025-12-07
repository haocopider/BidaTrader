using BidaTrader.Server.Services;
using BidaTraderShared.Data.DTOs;
using BidaTraderShared.Data.Models;
using BidaTraderShared.Data.Services;
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

    [HttpGet]
    public async Task<ActionResult<ProductPagedResponseDto>> GetAllProduct(
        [FromQuery] int? categoryId,
        [FromQuery] string? searchKey,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10) // ⬅️ THAM SỐ PHÂN TRANG
    {
        // 1. GỌI SERVICE LẤY DỮ LIỆU VÀ TỔNG SỐ LƯỢNG (Tối ưu tại DB)
        var (products, totalCount) = await ((ProductService)_productService).GetProductsForPaginationAsync(
            categoryId, searchKey, pageIndex, pageSize);

        if (products == null)
        {
            // Trả về DTO rỗng nếu không có dữ liệu
            return Ok(new ProductPagedResponseDto());
        }

        // 2. Mapping Entity -> DTO (Output)
        var dtos = products.Select(p => new ProductListDto
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
        var pagedResponse = new ProductPagedResponseDto
        {
            Items = dtos,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(pagedResponse); // ⬅️ TRẢ VỀ DTO PHÂN TRANG
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductListDto>> GetProduct(int id)
    {
        var product = await _productService.GetItemByIdAsync(id);

        // 1. Sửa Status Code: 404 Not Found là chuẩn cho tài nguyên không tồn tại
        if (product == null) return NotFound("Không tìm thấy sản phẩm.");

        // 2. Mapping Entity -> DTO (Output)
        var dto = new ProductListDto
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
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreateUpdateDto dto)
    {
        // Mapping DTO -> Entity
        var productModel = new Product
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            StoreId = dto.StoreId
        };

        // Gọi ProductService (Logic UpdatedAt sẽ chạy ở đây)
        var result = await _productService.UpdateItemAsync(productModel);

        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrduct([FromBody] ProductCreateUpdateDto dto)
    {
        // Mapping DTO -> Entity
        var productModel = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            StoreId = dto.StoreId
        };
        // Gọi ProductService (Logic CreatedAt sẽ chạy ở đây)
        var result = await _productService.CreateItemAsync(productModel);
        if (!result) return BadRequest("Could not create product.");
        return CreatedAtAction(nameof(GetAllProduct), new { id = productModel.Id }, productModel);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        bool pw = await _productService.DeleteItemAsync(id);
        if (!pw) return BadRequest("Xóa sản phẩm thất bại");
        return Ok();
    }
}