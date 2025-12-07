using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BidaTraderShared.Data.DTOs
{
    public class ProductListDto
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int? Quantity { get; set; }

        public double? Rating { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string? ImageUrl { get; set; } // URL ảnh đại diện
        public string CategoryName { get; set; } = "N/A"; // Tên danh mục
    }
}
