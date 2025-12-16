using BidaTrader.Client.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection; // Cần thêm namespace này
using System.Net.Http.Headers;

namespace BidaTrader.Client.Auth // Hoặc namespace đúng của bạn
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IServiceProvider _serviceProvider; // 1. Thay IAuthService bằng IServiceProvider

        public AuthHeaderHandler(ILocalStorageService localStorage, IServiceProvider serviceProvider)
        {
            _localStorage = localStorage;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Gửi request
            var response = await base.SendAsync(request, cancellationToken);

            // --- Phần 2: Xử lý 401 Unauthorized (Sửa đổi) ---
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Tránh vòng lặp vô tận nếu chính API Refresh Token bị lỗi
                if (request.RequestUri!.AbsolutePath.Contains("refresh-token") ||
                    request.RequestUri!.AbsolutePath.Contains("login"))
                {
                    return response;
                }

                var authService = _serviceProvider.GetRequiredService<IAuthService>();

                // Gọi Refresh Token
                var newToken = await authService.RefreshToken();

                if (!string.IsNullOrEmpty(newToken))
                {
                    // Gán token mới và thử lại request cũ
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }
}