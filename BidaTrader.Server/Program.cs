using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
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

builder.Services.AddScoped(typeof(IService<>), typeof(ServerService<>));
builder.Services.AddScoped<ProductService, ProductService>();
builder.Services.AddScoped<IService<Category>, CategoryService>();
builder.Services.AddScoped<IService<Brand>, BrandService>();
builder.Services.AddScoped<IService<Account>, AccountService>();
builder.Services.AddScoped<IService<Post>, PostService>();
builder.Services.AddScoped<IService<Store>, StoreService>();
builder.Services.AddScoped<CartService, CartService>();
builder.Services.AddScoped<OrderService, OrderService>();


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

// Cập nhật CORS Policy 
builder.Services.AddAuthorization(options =>
{
    // ================= ROLE BASE =================

    options.AddPolicy("FullFeat", policy =>
        policy.RequireRole("ADMIN")
    );

    options.AddPolicy("StoreOwner", policy =>
        policy.RequireRole("STORE")
    );

    options.AddPolicy("ForCustomer", policy =>
        policy.RequireRole("CUSTOMER")
    );

    options.AddPolicy("ForShipper", policy =>
        policy.RequireRole("SHIPPER")
    );

    // ================= COMBINED ROLE =================

    options.AddPolicy("AdminOrStore", policy =>
        policy.RequireRole("ADMIN", "STORE_OWNER")
    );

    options.AddPolicy("StoreOrCustomer", policy =>
        policy.RequireRole("STORE", "CUSTOMER")
    );

    options.AddPolicy("ForAll", policy =>
        policy.RequireRole("ADMIN", "STORE", "CUSTOMER")
    );

    // ================= STORE STATUS =================

    options.AddPolicy("ActiveStore", policy =>
        policy.RequireRole("STORE_OWNER")
              .RequireClaim("StoreActive", "True")
    );

    // ================= AUTHENTICATED =================

    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser()
    );

    // ================= PERMISSION BASE =================

    options.AddPolicy("PRODUCT.CREATE", policy =>
        policy.RequireClaim("Permission", "PRODUCT.CREATE")
    );

    options.AddPolicy("PRODUCT.UPDATE", policy =>
        policy.RequireClaim("Permission", "PRODUCT.UPDATE")
    );

    options.AddPolicy("PRODUCT.DELETE", policy =>
        policy.RequireClaim("Permission", "PRODUCT.DELETE")
    );
});


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