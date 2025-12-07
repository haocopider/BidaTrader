using BidaTraderShared.Data.Services;
using System.Net.Http.Json;

namespace BidaTrader.Client.Services
{
    public class ClientGenericService<T> : IService<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiEndpoint;

        public ClientGenericService(HttpClient httpClient, string endpointName)
        {
            _httpClient = httpClient;
            _apiEndpoint = $"api/{endpointName}";
        }

        public async Task<List<T>?> GetItemsAsync(string? queryString = null)
        {
            var url = _apiEndpoint;
            if (!string.IsNullOrEmpty(queryString))
            {
                url += queryString; // Nối chuỗi truy vấn (VD: ?categoryId=5)
            }
            try
            {
                return await _httpClient.GetFromJsonAsync<List<T>>(url);
            }
            catch { return null; }
        }

        // ... Các method Create, Update, Delete giữ nguyên như cũ
        public async Task<T?> GetItemByIdAsync(int id) =>
            await _httpClient.GetFromJsonAsync<T>($"{_apiEndpoint}/{id}");

        public async Task<bool> CreateItemAsync(T item) =>
            (await _httpClient.PostAsJsonAsync(_apiEndpoint, item)).IsSuccessStatusCode;

        public async Task<bool> UpdateItemAsync(T item)
        {
            // Logic lấy ID qua reflection để gọi PUT /api/products/{id}
            var idProp = item.GetType().GetProperty("Id");
            var id = idProp?.GetValue(item);
            return (await _httpClient.PutAsJsonAsync($"{_apiEndpoint}/{id}", item)).IsSuccessStatusCode;
        }

        public async Task<bool> DeleteItemAsync(int id) =>
            (await _httpClient.DeleteAsync($"{_apiEndpoint}/{id}")).IsSuccessStatusCode;
    }
}