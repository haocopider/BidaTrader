using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.DTOs
{
    public class ProductFilter
    {
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? StoreId { get; set; }
        public string? ProductName { get; set; }
        public string? StoreName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? Latest { get; set; }
        public bool? Highest { get; set; }
        public float? Rating { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}
