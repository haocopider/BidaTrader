using BidaTrader.Server.Helpers;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace BidaTrader.Server.Services
{
    public class AccountService : ServerService<Account>
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AccountService(AppDbContext context, IConfiguration configuration, IWebHostEnvironment env) : base(context)
        {
            _configuration = configuration;
            _env = env;
        }

        // --- 1. Lấy thông tin chi tiết Account (bao gồm xử lý logic null) ---
        public async Task<AccountDto?> GetAccountDetailAsync(int userId)
        {
            var account = await _context.Accounts.FindAsync(userId);
            if (account == null) return null;

            return MapToDto(account);
        }

        public async Task<AccountDto?> GetAccountByUidAsync(string uid)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Uid == uid);
            if (account == null) return null;

            return MapToDto(account);
        }

        // Helper Map DTO (Dùng nội bộ để tránh lặp code)
        private AccountDto MapToDto(Account account)
        {
            return new AccountDto
            {
                Id = account.Id,
                UID = account.Uid,
                UserName = account.UserName,
                Email = account.Email,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Phone = account.Phone,
                Address = account.Address,
                AvatarUrl = account.AvatarUrl,
                IsActive = account.IsActive,
                DateOfBirth = account.DateOfBirth,
                // Không trả về PasswordHash
            };
        }

        // --- 2. Cập nhật Profile ---
        public async Task<bool> UpdateProfileAsync(int userId, ProfileDto dto)
        {
            var account = await _context.Accounts.FindAsync(userId);
            if (account == null) return false;

            account.FirstName = dto.FirstName;
            account.LastName = dto.LastName;
            account.Email = dto.Email;
            account.Phone = dto.Phone;
            account.Address = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.Passcode))
            {
                account.Passcode = dto.Passcode;
            }

            // Xử lý ảnh đại diện
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl) && dto.AvatarUrl.StartsWith("data:image"))
            {
                string savedPath = await SaveImageToFolder(dto.AvatarUrl, account.UserName);
                if (!string.IsNullOrEmpty(savedPath))
                {
                    account.AvatarUrl = savedPath;
                }
            }

            _context.Accounts.Update(account);
            return await _context.SaveChangesAsync() > 0;
        }

        // --- 3. Lấy danh sách phân trang (Admin) ---
        public async Task<AccountPerPage> GetAccountsPaginationAsync(string? username, int pageIndex, int pageSize)
        {
            var query = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(a => a.UserName.Contains(username));
            }

            int totalItems = await query.CountAsync();

            var accounts = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new AccountDto // Select thẳng ra DTO để tối ưu
                {
                    Id = p.Id,
                    UID = p.Uid,
                    UserName = p.UserName,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return new AccountPerPage
            {
                Accounts = accounts,
                TotalCount = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        // --- 4. Logic xóa tài khoản ---
        public async Task<bool> DeleteAccountAsync(int id)
        {
            // Có thể thêm logic check ràng buộc (ví dụ: đang có đơn hàng thì không cho xóa)
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return false;

            _context.Accounts.Remove(account);
            return await _context.SaveChangesAsync() > 0;
        }

        // --- 5. Logic Passcode & Password ---
        public async Task<bool> HasPasscodeAsync(int userId)
        {
            var account = await _context.Accounts
                .Where(a => a.Id == userId)
                .Select(a => a.Passcode) // Chỉ lấy cột Passcode để nhẹ
                .FirstOrDefaultAsync();

            return !string.IsNullOrEmpty(account);
        }

        public async Task<bool> CheckPasscodeAsync(int userId, string passcode)
        {
            var currentPasscode = await _context.Accounts
                .Where(a => a.Id == userId)
                .Select(a => a.Passcode)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(currentPasscode)) return false;
            return currentPasscode == passcode;
        }

        public async Task<bool> ChangePasswordSecureAsync(int userId, string newPassword, string passcode)
        {
            var account = await _context.Accounts.FindAsync(userId);
            if (account == null) return false;

            if (account.Passcode != passcode) return false;

            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Accounts.Update(account);
            return await _context.SaveChangesAsync() > 0;
        }

        // --- 6. Logic Forgot Password (Giữ nguyên logic cũ của bạn, chỉ copy vào đây) ---
        public async Task<string> SendForgotPasswordOtpAsync(string email)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null) return "EMAIL_NOT_FOUND";

            var otp = new Random().Next(100000, 999999).ToString();
            account.PasswordResetToken = otp;
            account.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(5);

            await _context.SaveChangesAsync(); // Lưu OTP trước khi gửi mail để tránh lỗi gửi xong ko lưu kịp

            string subject = "[BidaTrader] Mã xác thực quên mật khẩu";
            string body = $@"<h3>Yêu cầu đặt lại mật khẩu</h3><p>Mã OTP: <b style='color:red'>{otp}</b></p>";

            try
            {
                await SendEmailAsync(email, subject, body);
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return $"MAIL_ERROR: {ex.Message}";
            }
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null) return false;
            return account.PasswordResetToken == otp && account.PasswordResetTokenExpiry > DateTime.UtcNow;
        }

        public async Task<string> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null) return "ACCOUNT_NOT_FOUND";

            if (account.PasswordResetToken != otp || account.PasswordResetTokenExpiry < DateTime.UtcNow)
                return "OTP_INVALID_OR_EXPIRED";

            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            account.PasswordResetToken = null;
            account.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();
            return "SUCCESS";
        }

        // --- Private Helpers ---
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // (Giữ nguyên code gửi mail cũ của bạn)
            var smtpConfig = _configuration.GetSection("Smtp");
            using var smtpClient = new SmtpClient
            {
                Host = smtpConfig["Host"],
                Port = int.Parse(smtpConfig["Port"] ?? "587"),
                EnableSsl = bool.Parse(smtpConfig["EnableSsl"] ?? "true"),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpConfig["UserName"], smtpConfig["Password"])
            };
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpConfig["UserName"]!, smtpConfig["FromName"] ?? "BidaTrader"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }

        private async Task<string> SaveImageToFolder(string base64String, string username)
        {
            // (Giữ nguyên code lưu ảnh cũ của bạn)
            if (string.IsNullOrEmpty(base64String) || !base64String.Contains("base64,")) return base64String;
            try
            {
                string webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string safeFileName = string.Join("_", username.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeFileName}_{DateTime.Now.Ticks}.jpg";
                var filePath = Path.Combine(uploadsFolder, fileName);

                var dataIndex = base64String.IndexOf("base64,") + 7;
                var buffer = Convert.FromBase64String(base64String.Substring(dataIndex));
                await File.WriteAllBytesAsync(filePath, buffer);

                return $"/uploads/avatars/{fileName}";
            }
            catch { return string.Empty; }
        }
    }
}