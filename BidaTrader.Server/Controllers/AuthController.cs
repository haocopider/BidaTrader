using BidaTrader.Server.Helpers; // Chứa PasswordHelper, UidHelper
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            // --- SINH UID THÔNG MINH ---
            // (Như logic tăng dần AA0001 -> AA0002)
            var lastAccount = await _context.Accounts.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            string newUid;
            try
            {
                // Giả định bạn có Helper class SequentialUidHelper (xem lại bài trước)
                newUid = SequentialUidHelper.GenerateNextUid(lastAccount?.Uid);
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = ex.Message });
            }

            // --- HASH PASSWORD (BCRYPT) ---
            // Sử dụng Helper hoặc BCrypt trực tiếp
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // --- TẠO ACCOUNT ---
            var newAccount = new Account
            {
                Uid = newUid,
                UserName = request.UserName,
                Email = request.Email ?? $"{request.UserName}@bidatrader.com",
                PasswordHash = passwordHash,
                Passcode = "",
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                FirstName = request.UserName,
                LastName = ""
            };

            try
            {
                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                // --- TẠO TOKEN ĐỂ LOGIN LUÔN ---
                var accessToken = GenerateJwtToken(newAccount);
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
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponseDto { IsSuccess = false, ErrorMessage = ex.Message });
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
            var accessToken = GenerateJwtToken(account);
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
            var newAccessToken = GenerateJwtToken(account);
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

        private string GenerateJwtToken(Account account)
        {
            // 1. Tạo Claims (Dữ liệu quan trọng để phân quyền)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()), // ID
                new Claim(ClaimTypes.Name, account.UserName),               // UserName
                new Claim("UID", account.Uid),                              // UID tùy chỉnh (AA0001)
                new Claim(ClaimTypes.Role, account.Role),                   // Role (Admin/Store/Customer)
                new Claim("IsActive", account.IsActive.ToString())          // Trạng thái hoạt động
            };

            if (account.StoreId.HasValue)
            {
                claims.Add(new Claim("StoreId", account.StoreId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
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