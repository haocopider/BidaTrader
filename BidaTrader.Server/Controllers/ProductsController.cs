using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }


    //Trang chủ
    [HttpGet("home")]
    public async Task<ActionResult<ProductPerPage>> HomePage(
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] string? pname,
        [FromQuery] string? sname,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? latest,
        [FromQuery] bool? highest,
        [FromQuery] float? rating,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 21)
    {
        var response = await ((ProductService)_productService).GetProductsForHomePageAsync(
            categoryId,
            brandId,
            pname,
            sname,
            minPrice,
            maxPrice,
            latest,
            highest,
            rating,
            pageIndex,
            pageSize
        );

        return Ok(response);
    }
    
    
    //Cửa hàng
    [HttpGet("mystore")]
    public async Task<ActionResult<ProductPerPage>> GetMyStore(
        [FromQuery] int pageIndex=1,
        [FromQuery] int pageSize=10)
    {
        var userId = GetCurrentUserId();
        var dtos = await _productService.GetMyStore(userId, pageIndex, pageSize);
        return Ok(dtos);
    }


    [HttpGet("store/{storeId}")]
    public async Task<ActionResult> StorePage(int storeId,
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] string? pname,
        [FromQuery] string? sname,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? latest,
        [FromQuery] bool? highest,
        [FromQuery] float? rating,
        [FromQuery] int pageIndex,
        [FromQuery] int pageSize)
    {
        var response = await ((ProductService)_productService).GetProductsForStorePageAsync(
            storeId,
            categoryId,
            brandId,
            pname,
            sname,
            minPrice,
            maxPrice,
            latest,
            highest,
            rating,
            pageIndex=1,
            pageSize=20
        );

        return Ok(response);
    }


    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        var response = await _productService.GetItemsAsync();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var product = await _productService.GetItemByIdAsync(id);
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrduct([FromBody] ProductCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();

            if (userId <= 0)
            {
                return Unauthorized("Không tìm thấy thông tin người dùng.");
            }

            var created = await _productService.CreateProductAsync(dto, userId);

            if (!created)
            {
                return BadRequest("Tạo mới sản phẩm thất bại. Vui lòng kiểm tra lại thông tin cửa hàng hoặc dữ liệu nhập.");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Lỗi Server: " + ex.Message);
        }
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto dto)
    {
        if (id != dto.Id) return BadRequest("ID không khớp.");
        var existingProduct = await _productService.GetItemByIdAsync(id);
        if (existingProduct == null) return NotFound("Không tìm thấy sản phẩm để cập nhật.");
        var updated = await _productService.UpdateItemAsync(dto);
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

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
        {
            return userId;
        }
        throw new Exception("User not found in token");
    }

}