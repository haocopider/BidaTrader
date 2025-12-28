using BidaTrader.Server.Helpers; // Chứa PasswordHelper, UidHelper
using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BidaTrader.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto request)
        {
            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Thông tin không hợp lệ." });

            // Kiểm tra trùng username
            if (await _context.Accounts.AnyAsync(u => u.UserName == request.UserName))
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Tên đăng nhập đã tồn tại." });

            var lastAccount = await _context.Accounts.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            string newUid;
            try
            {
                newUid = SequentialUidHelper.GenerateNextUid(lastAccount?.Uid);
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = ex.Message });
            }


            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // --- TẠO ACCOUNT ---
            var newAccount = new Account
            {
                Uid = newUid,
                UserName = request.UserName,
                Email = request.Email ?? $"{request.UserName}@bidatrader.com",
                PasswordHash = passwordHash,
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                FirstName = request.UserName,
                LastName = "",
                Phone = "",
                Address = "",
                AvatarUrl = ""
            };

            try
            {
                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                // --- TẠO TOKEN ĐỂ LOGIN LUÔN ---
                var accessToken = await GenerateJwtToken(newAccount);
                var refreshToken = GenerateRefreshToken();

                // Lưu Refresh Token
                newAccount.RefreshToken = refreshToken;
                newAccount.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Token = accessToken,
                    RefreshToken = refreshToken
                });
            }
            catch (DbUpdateException dbEx)
            {
                // Lấy chi tiết lỗi từ InnerException để biết chính xác cột nào gây lỗi
                var errorMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Lỗi Database: " + errorMsg
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.UserName == request.UserName);

            // Kiểm tra tồn tại và Hash Password
            if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Sai thông tin đăng nhập." });
            }

            if (account.IsActive == false)
                return Unauthorized(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Tài khoản đã bị khóa." });

            // Sinh Token mới
            var accessToken = await GenerateJwtToken(account);
            var refreshToken = GenerateRefreshToken();

            // Cập nhật Refresh Token vào DB
            account.RefreshToken = refreshToken;
            account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Token = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Token không hợp lệ." });

            // Tìm user có refresh token này
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            // Kiểm tra tính hợp lệ
            if (account == null || account.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Phiên hết hạn." });
            }

            // Cấp Token mới
            var newAccessToken = await GenerateJwtToken(account);
            var newRefreshToken = GenerateRefreshToken();

            // Xoay vòng Refresh Token (Token Rotation) để bảo mật
            account.RefreshToken = newRefreshToken;
            account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Gia hạn thêm
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        // =========================================================================
        // HELPERS (HÀM HỖ TRỢ)
        // =========================================================================

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateJwtToken(Account account)
        {
            var permissions = await _context.AccountRoles
                .Where(ar => ar.AccountId == account.Id)
                .SelectMany(ar => ar.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            var claims = new List<Claim>
            {
                // ===== Standard JWT claims =====
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                new Claim(ClaimTypes.NameIdentifier, account.Uid),
                new Claim(ClaimTypes.Name, account.UserName),
                new Claim(ClaimTypes.Role, account.Role.Trim()),
                new Claim("permissions", string.Join(",", permissions))
            };

            // ===== Store-specific claims =====
            if (account.Role == "Store")
            {
                claims.Add(new Claim("IsActive", account.IsActive ? "True" : "False"));

                if (account.StoreId.HasValue)
                {
                    claims.Add(new Claim("StoreId", account.StoreId.Value.ToString()));
                }
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}