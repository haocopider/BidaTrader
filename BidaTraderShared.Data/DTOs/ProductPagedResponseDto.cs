using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTraderShared.Data.DTOs
{
    public class ProductPagedResponseDto
    {
        // Danh sách sản phẩm trên trang hiện tại
        public List<ProductListDto> Items { get; set; } = new List<ProductListDto>();

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } // Tổng số lượng sản phẩm TRONG DB (QUAN TRỌNG)

        // Thuận tiện cho UI tính toán tổng số trang
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
