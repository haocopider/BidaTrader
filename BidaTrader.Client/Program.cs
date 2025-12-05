using BidaTrader.Client;
using BidaTrader.Client.Auth;
using BidaTrader.Client.Services;
using BidaTraderShared.Data.Models;
using BidaTraderShared.Data.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Authorization / authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Auth service and HTTP handler
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<AuthHeaderHandler>();

// Configure a named HttpClient for calling the server API.
// Set ApiBaseUrl via configuration (appsettings or environment) or fallback to localhost used by the server launchSettings.
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7049/";
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBase);
}).AddHttpMessageHandler<AuthHeaderHandler>();

// Provide default HttpClient for components that inject HttpClient
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
builder.Services.AddScoped<IService<Product>, ProductService>();
builder.Services.AddScoped<IService<Category>, CategoryService>();
builder.Services.AddScoped<IService<Brand>, BrandService>();

await builder.Build().RunAsync();
