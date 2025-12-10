using BidaTraderShared.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BidaTraderShared.Data.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không hợp lệ.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải dài từ 8 đến 100 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d\s]).{8,}$",
    ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm: chữ hoa, chữ thường, số và ký tự đặc biệt.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Vai trò không được vượt quá 20 ký tự.")]
        public string Role { get; set; } = "Customer";

        [Required(ErrorMessage = "Trạng thái kích hoạt là bắt buộc.")]
        public bool? IsActive { get; set; }

        public virtual Customer? Customer { get; set; }
    }

    public class AccountPerPage
    {
        public List<AccountDto> Accounts { get; set; } = new List<AccountDto>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
    public class AccountRoleUpdateDto
    {
        // ID của Account cần cập nhật (thường được truyền qua URL hoặc Body)
        [Required(ErrorMessage = "ID tài khoản là bắt buộc.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Vai trò không được vượt quá 20 ký tự.")]
        // Bạn nên kiểm tra giá trị Role này có hợp lệ không (ví dụ: "Admin", "Store", "Customer")
        public string Role { get; set; } = "Customer";

        [Required(ErrorMessage = "Trạng thái kích hoạt là bắt buộc.")]
        public bool IsActive { get; set; } = true;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải dài từ 8 đến 100 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d\s]).{8,}$",
            ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự, bao gồm: chữ hoa, chữ thường, số và ký tự đặc biệt.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu mới là bắt buộc.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không hợp lệ.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Tùy chọn: Thêm trường nhớ mật khẩu
        public bool RememberMe { get; set; }
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Tên đăng nhập phải dài từ 5 đến 50 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$",
            ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái, số, dấu gạch dưới (_) và dấu chấm (.).")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải dài từ 8 đến 100 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d\s]).{8,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm: chữ hoa, chữ thường, số và ký tự đặc biệt.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

}
