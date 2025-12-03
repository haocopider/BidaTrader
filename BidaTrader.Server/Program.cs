using BidaTraderShared.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === BẮT ĐẦU CẤU HÌNH JWT ===
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();
// === KẾT THÚC CẤU HÌNH JWT ===

// Cập nhật CORS Policy (sử dụng cấu hình an toàn hơn)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        b => b.WithOrigins(builder.Configuration["Jwt:Audience"]) // Chỉ cho phép Client
               .AllowAnyMethod()
               .AllowAnyHeader());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Thay thế UseCors cũ bằng UseCors mới
app.UseCors("AllowBlazorClient");

// THÊM 2 DÒNG NÀY (Thứ tự rất quan trọng)
app.UseAuthentication(); // 1. Xác thực (bạn là ai?)
app.UseAuthorization(); // 2. Phân quyền (bạn được làm gì?)

app.MapControllers();

app.Run();