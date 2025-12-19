using BidaTrader.Shared.Models;
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
        public string? LogoUrl { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? SocialLinks { get; set; }
        public bool IsActive { get; set; }
        public bool IsRecycled { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string AvatarUrl { get; set; }
        public string CoverUrl { get; set; }
        public int Followers { get; set; }
        public double Rating { get; set; }
        public int TotalProducts { get; set; }
        public DateTime JoinDate { get; set; }
        public List<string> BannerImages { get; set; }
    }

    public class StoreDetailDto
    {
        public int Id { get; set; }
        public string StoreName { get; set; }
        public string AvatarUrl { get; set; }
        public string CoverUrl { get; set; }
        public string Address { get; set; }
        public int Followers { get; set; }
        public double Rating { get; set; }
        public int TotalProducts { get; set; }
        public DateTime JoinDate { get; set; }
        public List<string> BannerImages { get; set; }
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
