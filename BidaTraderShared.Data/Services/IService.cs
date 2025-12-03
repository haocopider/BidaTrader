namespace BidaTraderShared.Data.Services
{
    public interface IService<T>
    {
        Task<List<T>?> GetItemsAsync();
        Task<T?> GetItemByIdAsync(int id);
        Task<bool> CreateItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(int id);
    }
}
