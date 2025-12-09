using BidaTraderShared.Data.DTOs;

namespace BidaTrader.Client.Services
{
    public interface IAuthService
    {
        Task<bool> Login(LoginDto loginModel);
        Task Logout();
        Task<RegisterDto> Register(RegisterDto registerModel);
    }
}
