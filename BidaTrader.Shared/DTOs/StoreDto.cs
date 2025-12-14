using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.DTOs
{
    public class StoreDto
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string StoreName { get; set; } = null!;

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Description { get; set; }

        public string? SocialLinks { get; set; }

        public bool IsActive { get; set; }

        public bool IsRecycled { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
    
    public class StorePerPage
    {
        public List<StoreDto> Stores { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
