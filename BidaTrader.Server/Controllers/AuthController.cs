using BidaTrader.Server.Helpers;
using BidaTrader.Server.Services;
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
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Thông tin không hợp lệ." });

            if (await _context.Accounts.AnyAsync(u => u.UserName == request.UserName))
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Tên đăng nhập đã tồn tại." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lastAccount = await _context.Accounts.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
                string newUid = SequentialUidHelper.GenerateNextUid(lastAccount?.Uid);

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var newAccount = new Account
                {
                    Uid = newUid,
                    UserName = request.UserName,
                    Email = request.Email ?? $"{request.UserName}@bidatrader.com",
                    PasswordHash = passwordHash,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    FirstName = request.UserName,
                    LastName = "",
                    Phone = "",
                    Address = "",
                    AvatarUrl = ""
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "CUSTOMER");

                var accountRole = new AccountRole
                {
                    AccountId = newAccount.Id,
                    RoleId = customerRole.Id
                };

                _context.AccountRoles.Add(accountRole);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new AuthResponseDto
                {
                    IsSuccess = true
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
            var account = await _context.Accounts
                .Include(u => u.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Sai thông tin đăng nhập." });
            }

            if (account.IsActive == false)
                return Unauthorized(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Tài khoản đã bị khóa." });

            var accessToken = GenerateJwtToken(account);
            var refreshToken = GenerateRefreshToken();

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

            var account = await _context.Accounts
                .Include(u => u.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (account == null || account.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Phiên hết hạn." });
            }

            var newAccessToken = GenerateJwtToken(account);
            var newRefreshToken = GenerateRefreshToken();

            account.RefreshToken = newRefreshToken;
            account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        private string GenerateJwtToken(Account account)
        {
            var role = account.AccountRoles?
                    .Select(ar => ar.Role)
                    .FirstOrDefault();

            var permissions = new List<string>();

            if (role != null && role.RolePermissions != null)
            {
                permissions = role.RolePermissions
                    .Where(rp => rp.Permission != null)
                    .Select(rp => rp.Permission.Code)
                    .Where(code => !string.IsNullOrEmpty(code))
                    .Distinct()
                    .ToList();
            }

            var avatarUrl = "https://localhost:7049" + (account.AvatarUrl ?? "");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Uid ?? ""),
                new Claim(ClaimTypes.Name, account.UserName ?? ""),

                new Claim("permissions", string.Join(",", permissions)),
                new Claim("avatarUrl", avatarUrl)
            };

            if (role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Code ?? ""));

                if (role.Code == "STORE")
                {
                    claims.Add(new Claim("IsActive", account.IsActive ? "True" : "False"));
                    if (account.StoreId.HasValue)
                    {
                        claims.Add(new Claim("StoreId", account.StoreId.Value.ToString()));
                    }
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "GUEST"));
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