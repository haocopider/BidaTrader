using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BidaTraderShared.Data.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int StoreId { get; set; }

        public int? Quantity { get; set; }

        public double? Rating { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = "N/A";
    }

    public class ProductPerPage
    {
        public List<ProductDto> Items { get; set; } = new List<ProductDto>();

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } // Tổng số lượng sản phẩm TRONG DB (QUAN TRỌNG)

        // Thuận tiện cho UI tính toán tổng số trang
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

}
