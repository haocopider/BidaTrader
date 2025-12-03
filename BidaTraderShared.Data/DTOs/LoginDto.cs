using System.ComponentModel.DataAnnotations;

namespace BidaTraderShared.Data.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50,ErrorMessage = "Tên đăng nhập không hợp lệ.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Tùy chọn: Thêm trường nhớ mật khẩu
        public bool RememberMe { get; set; }
    }
}