using System.ComponentModel.DataAnnotations;

namespace BidaTraderShared.Data.DTOs
{
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
}