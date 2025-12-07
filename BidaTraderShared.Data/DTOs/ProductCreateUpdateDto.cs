using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BidaTraderShared.Data.DTOs
{
    public class ProductCreateUpdateDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int StoreId { get; set; } // Store nào đăng bán
    }
}
