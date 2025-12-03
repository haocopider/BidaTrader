using BidaTrader.Client.Auth;
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

        // DTO để nhận phản hồi từ API
        private class LoginResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }
            [JsonPropertyName("userName")]
            public string UserName { get; set; }
        }

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> Login(string username, string password)
        {
            if (_httpClient.BaseAddress == null)
            {
                // Fail fast and surface a clearer message for debugging
                throw new InvalidOperationException("HttpClient.BaseAddress is null. Configure the API base address in Program.cs (ApiBaseUrl) or register a client with a BaseAddress.");
            }

            var loginRequest = new { UserName = username, Password = password };

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
            await _localStorage.SetItemAsync("userName", loginResponse.UserName);

            // Thông báo cho Blazor biết trạng thái xác thực đã thay đổi
            await ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(loginResponse.Token);

            return true;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("userName");

            // Thông báo cho Blazor biết trạng thái đã thay đổi
            await ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }
    }
}