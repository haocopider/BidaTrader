using BidaTrader.Client.Auth;
using BidaTrader.Shared.DTOs;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BidaTrader.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private class LoginResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("tokenExpiryUtc")]
            public DateTime TokenExpiryUtc { get; set; }
        }

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> Login(LoginDto loginModel)
        {
            if (_httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HttpClient.BaseAddress is null. Configure the API base address in Program.cs (ApiBaseUrl) or register a client with a BaseAddress.");
            }

            var loginRequest = new { UserName = loginModel.UserName, Password = loginModel.Password };

            // Compose absolute Uri from the configured BaseAddress to avoid invalid request URI errors on WASM.
            var requestUri = new Uri(_httpClient.BaseAddress, "api/Auth/login");

            var response = await _httpClient.PostAsJsonAsync(requestUri, loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                return false;
            }

            // Lưu token vào Local Storage
            await _localStorage.SetItemAsync("authToken", loginResponse.Token);

            await _localStorage.SetItemAsync("tokenExpiryUtc", loginResponse.TokenExpiryUtc);

            // Thông báo cho Blazor biết trạng thái xác thực đã thay đổi
            await ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(loginResponse.Token);

            return true;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("tokenExpiryUtc");
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<RegisterDto> Register(RegisterDto registerModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerModel);

                // Đọc kết quả trả về từ Server
                var result = await response.Content.ReadFromJsonAsync<RegisterDto>();
                return result ?? new RegisterDto { IsSuccess = false, ErrorMessage = "Lỗi không xác định." };
            }
            catch (Exception ex)
            {
                return new RegisterDto { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<string?> RefreshToken()
        {
            try
            {
                // 1. Lấy Token cũ từ LocalStorage
                var token = await _localStorage.GetItemAsync<string>("authToken");
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

                // Nếu không có token thì không thể refresh -> Logout
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                {
                    await Logout();
                    return null;
                }

                // 2. Gửi yêu cầu lên Server
                var refreshDto = new RefreshTokenDto
                {
                    Token = token,
                    RefreshToken = refreshToken
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", refreshDto);

                // 3. Xử lý kết quả
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                    if (result != null && result.IsSuccess && !string.IsNullOrEmpty(result.Token))
                    {
                        // THÀNH CÔNG: Lưu Token mới đè lên cái cũ
                        await _localStorage.SetItemAsync("authToken", result.Token);
                        await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);

                        // Trả về token mới để HttpInterceptor sử dụng ngay lập tức
                        return result.Token;
                    }
                }
            }
            catch (Exception)
            {
                // Gặp lỗi mạng hoặc lỗi server
            }
            await Logout();
            return null;
        }
    }
}