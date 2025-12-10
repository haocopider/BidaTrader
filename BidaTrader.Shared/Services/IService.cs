namespace BidaTrader.Shared.Services
{
    public interface IService<T>
    {
        Task<List<T>?> GetItemsAsync(string? queryString = null);
        Task<T?> GetItemByIdAsync(int id);
        Task<bool> CreateItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(int id);
    }
}
