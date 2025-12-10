using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        // DTO trả về khi login thành công
        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public DateTime TokenExpiryUtc { get; set; }
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterDto>> Register([FromBody] RegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new RegisterDto { IsSuccess = false, ErrorMessage = "UserName và Password là bắt buộc." });
            }

            if (await _context.Accounts.AnyAsync(u => u.UserName == request.UserName))
            {
                return BadRequest(new RegisterDto { IsSuccess = false, ErrorMessage = "Tên đăng nhập đã tồn tại." });
            }

            var newAccount = new Account
            {
                UserName = request.UserName,
                Email = request.Email ?? $"{request.UserName}@bidatrader.com",
                PasswordHash = request.Password,
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Ok(new RegisterDto { IsSuccess = true });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.UserName) || string.IsNullOrWhiteSpace(loginRequest.Password))
                return BadRequest(new { message = "Missing credentials" });

            var account = await _context.Accounts.SingleOrDefaultAsync(a => a.UserName == loginRequest.UserName);
            if (account == null) return Unauthorized(new { message = "User or password incorrect." });

            if (loginRequest.Password != account.PasswordHash)
                return Unauthorized(new { message = "User or password incorrect." });

            // Tạo access token
            var token = GenerateJwtToken(account, out DateTime expiresUtc);

            //// Tạo refresh token và lưu vào db (hoặc bảng token riêng)
            //var refreshToken = CreateRandomToken();
            //account.RefreshToken = refreshToken;
            //account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 14));

           // await _context.SaveChangesAsync();

            //// Option 1 (đề xuất): Gửi refresh token dưới dạng HttpOnly cookie (an toàn hơn)
            //var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            //{
            //    HttpOnly = true,
            //    //Expires = account.RefreshTokenExpiryTime,
            //    Secure = true, // production: true
            //    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
            //};
            //Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            var resp = new LoginResponse
            {
                Token = token,
                UserName = account.UserName,
                Role = account.Role,
                TokenExpiryUtc = expiresUtc
            };

            return Ok(resp);
        }

        // Refresh token endpoint: sẽ lấy refresh token từ cookie hoặc body
        //[HttpPost("refresh-token")]
        //public async Task<IActionResult> RefreshToken()
        //{
        //    // Lấy token từ cookie (nếu bạn đã set cookie)
        //    string? refreshToken = Request.Cookies["refreshToken"];

        //    // Nếu client không dùng cookie, bạn có thể chấp nhận token trong body:
        //    if (string.IsNullOrEmpty(refreshToken))
        //    {
        //        return BadRequest(new { message = "No refresh token present." });
        //    }

        //    // Tìm account có refresh token
        //    var account = await _context.Accounts.SingleOrDefaultAsync(a => a.RefreshToken == refreshToken);
        //    if (account == null)
        //    {
        //        return Unauthorized(new { message = "Invalid refresh token." });
        //    }

        //    if (account.RefreshTokenExpiryTime == null || account.RefreshTokenExpiryTime <= DateTime.UtcNow)
        //    {
        //        return Unauthorized(new { message = "Refresh token expired." });
        //    }

        //    // Tạo access token mới
        //    var newAccessToken = GenerateJwtToken(account, out DateTime newExpiresUtc);

        //    // (Tùy chọn) rotate refresh token: tạo 1 refresh token mới, lưu lại
        //    var newRefreshToken = CreateRandomToken();
        //    account.RefreshToken = newRefreshToken;
        //    account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 14));
        //    await _context.SaveChangesAsync();

        //    // Ghi cookie mới
        //    var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
        //    {
        //        HttpOnly = true,
        //        Expires = account.RefreshTokenExpiryTime,
        //        Secure = true,
        //        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
        //    };
        //    Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

        //    return Ok(new
        //    {
        //        token = newAccessToken,
        //        tokenExpiryUtc = newExpiresUtc
        //    });
        //}

        // Revoke token (logout) - huỷ refresh token cho account đang dùng
        //[HttpPost("revoke")]
        //public async Task<IActionResult> RevokeToken()
        //{
        //    // Lấy refresh token từ cookie
        //    string? refreshToken = Request.Cookies["refreshToken"];
        //    if (string.IsNullOrEmpty(refreshToken))
        //    {
        //        return BadRequest(new { message = "No refresh token present." });
        //    }

        //    var account = await _context.Accounts.SingleOrDefaultAsync(a => a.RefreshToken == refreshToken);
        //    if (account == null) return NotFound();

        //    account.RefreshToken = null;
        //    account.RefreshTokenExpiryTime = null;

        //    // Xoá cookie
        //    Response.Cookies.Delete("refreshToken");

        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Logged out" });
        //}

        // ---------------- Helper methods ----------------

        private string GenerateJwtToken(Account account, out DateTime expiresUtc)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, account.UserName),
                new Claim(JwtRegisteredClaimNames.Email, account.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Role: dùng ClaimTypes.Role để .NET/Blazor hiểu
            if (!string.IsNullOrWhiteSpace(account.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, account.Role));
            }

            var minutes = _configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 60);
            expiresUtc = DateTime.UtcNow.AddMinutes(minutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresUtc,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        //private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        //{
        //    using var hmac = new HMACSHA512();
        //    passwordSalt = hmac.Key;
        //    passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        //}

        //private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        //{
        //    using var hmac = new HMACSHA512(storedSalt);
        //    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        //    if (computedHash.Length != storedHash.Length) return false;
        //    for (int i = 0; i < computedHash.Length; i++)
        //    {
        //        if (computedHash[i] != storedHash[i]) return false;
        //    }
        //    return true;
        //}

        // Tạo refresh token ngẫu nhiên
        //private string CreateRandomToken()
        //{
        //    var randomNumber = new byte[64];
        //    using var rng = RandomNumberGenerator.Create();
        //    rng.GetBytes(randomNumber);
        //    return Convert.ToBase64String(randomNumber);
        //}
    }
}
