namespace BidaTrader.Client.Services
{
    public interface IAuthService
    {
        Task<bool> Login(string username, string password);
        Task Logout();
    }
}
