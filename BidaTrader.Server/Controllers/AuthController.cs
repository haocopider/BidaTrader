using BidaTraderShared.Data.DTOs;
using BidaTraderShared.Data.Models;
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

        [HttpPost("register")]
        public async Task<ActionResult<RegisterDto>> Register(RegisterDto request)
        {
            // 1. Kiểm tra User đã tồn tại chưa
            if (await _context.Accounts.AnyAsync(u => u.UserName == request.UserName))
            {
                return BadRequest(new RegisterDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Tên đăng nhập đã tồn tại."
                });
            }

            // 2. Tạo Password Hash & Salt
            //CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // 3. Tạo Account Entity
            var newAccount = new Account
            {
                UserName = request.UserName,
                // Lưu Hash và Salt (Bạn cần sửa Model Account để lưu byte[] hoặc chuyển sang string base64)
                // GIẢ SỬ Model Account của bạn lưu PasswordHash là string, ta convert sang Base64
                PasswordHash = request.Password,
                //RandomKey = Convert.ToBase64String(passwordSalt), // Dùng trường RandomKey để lưu Salt
                Role = "Customer", // Mặc định là Khách hàng
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync(); // Lưu để lấy AccountId

            // 4. Tạo Profile Customer tương ứng
            var newCustomer = new Customer
            {
                AccountId = newAccount.Id,
                Email = request.UserName + "@bidatrader.com", // Email tạm hoặc thêm trường Email vào RegisterDto
                CreatedAt = DateTime.Now
            };
            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return Ok(new RegisterDto { IsSuccess = true });
        }

        // Hàm hỗ trợ Hash Password (HMACSHA512)
        //private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        //{
        //    using (var hmac = new HMACSHA512())
        //    {
        //        passwordSalt = hmac.Key; // Random Key sinh ra bởi HMAC
        //        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        //    }
        //}
    }
}