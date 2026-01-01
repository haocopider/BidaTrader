using BidaTrader.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BidaTrader.Shared.DTOs
{
    public class StoreDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string? OwnerId { get; set; }
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

    public class CreateStoreDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cửa hàng")]
        [StringLength(100, ErrorMessage = "Tên cửa hàng không được quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ lấy hàng")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại cửa hàng")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        public string? LogoUrl { get; set; } // Chuỗi Base64 hoặc URL ảnh
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
