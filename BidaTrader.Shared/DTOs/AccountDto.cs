using System.ComponentModel.DataAnnotations;

namespace BidaTrader.Shared.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string UID { get; set; }
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không hợp lệ.")]
        public string UserName { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải dài từ 8 đến 100 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d\s]).{8,}$",
    ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm: chữ hoa, chữ thường, số và ký tự đặc biệt.")]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Vai trò không được vượt quá 20 ký tự.")]
        public string Role { get; set; } = "Customer";

        [Required(ErrorMessage = "Trạng thái kích hoạt là bắt buộc.")]
        public bool? IsActive { get; set; }
        public string? Passcode { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }

        public DateOnly? DateOfBirth { get; set; }
    }
  
    public class ProfileDto
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Role { get; set; } = "Customer";
        public string? Passcode { get; set; }
        public bool IsActive { get; set; }
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
        [Required(ErrorMessage = "Vui lòng nhập Passcode")]
        public string CurrentPasscode { get; set; } = "";

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

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";
    }

    public class VerifyOtpDto
    {
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        public string Otp { get; set; } = "";
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = "";
        public string Otp { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        public string NewPassword { get; set; } = "";

        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = "";
    }

    public class UpdateRolePermissionsDto
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public class RoleWithPermissionsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> AssignedPermissionIds { get; set; } = new();
    }
}
