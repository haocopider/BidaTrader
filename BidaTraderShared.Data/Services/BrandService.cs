using BidaTraderShared.Data.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace BidaTraderShared.Data.Services
{
    public class BrandService : IService<Brand>
    {
        private readonly HttpClient _httpClient;

        public BrandService(HttpClient httpClient) => _httpClient = httpClient;
        public async Task<bool> CreateItemAsync(Brand item)
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", item);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/brands/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Brand?> GetItemByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Brand>($"api/brands/{id}");
        }

        public async Task<List<Brand>?> GetItemsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Brand>>("api/brands");
        }

        public async Task<bool> UpdateItemAsync(Brand item)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/brands/{item.Id}", item);
            return response.IsSuccessStatusCode;
        }
    }
}
