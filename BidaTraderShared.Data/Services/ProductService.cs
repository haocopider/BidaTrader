using BidaTraderShared.Data.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace BidaTraderShared.Data.Services
{
    public class ProductService : IService<Product>
    {
        private readonly HttpClient _httpClient;
        public ProductService(HttpClient httpClient) => _httpClient = httpClient;
        public async Task<bool> CreateItemAsync(Product item)
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", item);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Product?> GetItemByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Product>($"api/products/{id}");
        }

        public async Task<List<Product>?> GetItemsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Product>>("api/products");
        }

        public async Task<bool> UpdateItemAsync(Product item)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/products/{item.Id}", item);
            return response.IsSuccessStatusCode;
        }
    }
}
