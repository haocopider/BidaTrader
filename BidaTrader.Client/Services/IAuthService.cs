using BidaTrader.Shared.DTOs;

namespace BidaTrader.Client.Services
{
    public interface IAuthService
    {
        Task<bool> Login(LoginDto loginModel);
        Task Logout();
        Task<string?> RefreshToken();
        Task<RegisterDto> Register(RegisterDto registerModel);
    }
}
