using Azure.Core;
using BidaTrader.Server.Helpers;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace BidaTrader.Server.Services
{
    public class AccountService : ServerService<Account>
    {
        private readonly IConfiguration _configuration;
        public AccountService(AppDbContext context, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
        }

        #region MailService
        public async Task<string> SendForgotPasswordOtpAsync(string email)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null)
            {
                return "EMAIL_NOT_FOUND";
            }


            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu vào DB
            account.PasswordResetToken = otp;
            account.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(5);
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            string subject = "[BidaTrader] Mã xác thực quên mật khẩu";
            string body = $@"
            <h3>Yêu cầu đặt lại mật khẩu</h3>
            <p>Mã OTP của bạn là: <b style='font-size:20px;color:red'>{otp}</b></p>
            <p>Mã này có hiệu lực trong 5 phút.</p>";

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

            if (account.PasswordResetToken == otp && account.PasswordResetTokenExpiry > DateTime.UtcNow)
            {
                Console.WriteLine(account.PasswordResetToken + account.PasswordResetTokenExpiry);
                return true;
            }

            return false;
        }

        public async Task<string> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null) return "ACCOUNT_NOT_FOUND";

            if (account.PasswordResetToken != otp || account.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return "OTP_INVALID_OR_EXPIRED";
            }

            account.PasswordHash = HashPasswordMethod(newPassword);

            account.PasswordResetToken = null;
            account.PasswordResetTokenExpiry = null;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return "SUCCESS";
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpConfig = _configuration.GetSection("Smtp");

            try
            {
                using var smtpClient = new SmtpClient
                {
                    // Lưu ý: Tên key phải khớp với JSON (Host)
                    Host = smtpConfig["Host"],
                    Port = int.Parse(smtpConfig["Port"] ?? "587"),
                    EnableSsl = bool.Parse(smtpConfig["EnableSsl"] ?? "true"),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        smtpConfig["UserName"],
                        smtpConfig["Password"]
                    )
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
            catch (Exception ex)
            {
                throw new Exception($"Gửi email thất bại: {ex.Message}");
            }
        }

        #endregion

        #region Phân quyền
        public async Task<List<RoleWithPermissionsDto>> GetAllRolesWithPermissionsAsync()
        {
            return await _context.Roles
                .Select(r => new RoleWithPermissionsDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    AssignedPermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        public async Task<bool> UpdateRolePermissionsAsync(UpdateRolePermissionsDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldPermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == dto.RoleId)
                    .ToListAsync();

                _context.RolePermissions.RemoveRange(oldPermissions);

                // Thêm quyền mới
                if (dto.PermissionIds != null && dto.PermissionIds.Any())
                {
                    var newPermissions = dto.PermissionIds.Select(permId => new RolePermission
                    {
                        RoleId = dto.RoleId,
                        PermissionId = permId
                    });
                    await _context.RolePermissions.AddRangeAsync(newPermissions);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        #endregion


        private string HashPasswordMethod(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<(List<Account> Accounts, int totalItems)> GetAccountWithPagination(string? username, string? role, int pageIndex=1, int pageSize = 10)
        {
            var query = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(a => a.UserName.Contains(username));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(a => a.Role.Contains(role));
            }

            int totalItems = await query.CountAsync();

            var pageItems = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (pageItems, totalItems);
        }
    
        public async Task<Account> GetAccountByUIDAsync(string uid)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Uid == uid);
        }

        public async Task<List<string>> GetPermissionAsync(int accountId)
        {
            return await _context.AccountRoles
                .Where(ar => ar.AccountId == accountId)
                .SelectMany(ar => ar.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();
        }
    }
}
