using BidaTraderShared.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        // DTO (Data Transfer Object) cho Đăng nhập
        public class LoginRequest
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            // CẢNH BÁO BẢO MẬT: Trong thực tế, bạn PHẢI băm (hash) mật khẩu
            // Ở đây chúng ta chỉ so sánh mật khẩu dạng văn bản thô (plaintext)
            var account = _context.Accounts.FirstOrDefault(c =>
                c.UserName == loginRequest.UserName && c.PasswordHash == loginRequest.Password);

            if (account == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });
            }

            // Nếu customer hợp lệ, tạo Token
            var token = GenerateJwtToken(account);

            // Trả về token và tên người dùng
            return Ok(new { token = token, userName = account.UserName });
        }

        private string GenerateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            string roleName = account.Role;
            if(account.Role == "Admin")
            {
                roleName = "Admin";
            }
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8), // Thời gian hết hạn
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}