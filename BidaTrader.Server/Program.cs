using BidaTrader.Server.Helpers;
using BidaTrader.Server.Services;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

builder.Services.AddScoped(typeof(IService<>), typeof(ServerService<>));
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IService<Category>, CategoryService>();
builder.Services.AddScoped<IService<Brand>, BrandService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<IService<Post>, PostService>();
builder.Services.AddScoped<IService<Store>, StoreService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<VnPayService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<MailHelper>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7103", // Blazor WASM (https)
                "http://localhost:5009"   // Blazor WASM (http)
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseFileServer();
app.UseStaticFiles();

app.UseCors("AllowBlazorClient");

app.UseAuthentication(); // 1. Xác thực
app.UseAuthorization(); // 2. Phân quyền

app.MapControllers();

app.Run();