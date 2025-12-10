using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;

namespace BidaTrader.Client.Services
{
    public interface IAuthService
    {
        Task<bool> Login(LoginDto loginModel);
        Task Logout();
        Task<RegisterDto> Register(RegisterDto registerModel);
    }
}
