using BidaTraderShared.Data.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace BidaTraderShared.Data.Services
{
    public class CategoryService : IService<Category>
    {
        private readonly HttpClient _httpClient;

        public CategoryService(HttpClient httpClient) => _httpClient = httpClient;
        public async Task<bool> CreateItemAsync(Category item)
        {
            var reponse = await _httpClient.PostAsJsonAsync($"api/categories", item);
            return reponse.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var reponse = await _httpClient.DeleteAsync($"api/categories/{id}");
            return reponse.IsSuccessStatusCode;
        }

        public async Task<Category?> GetItemByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Category>($"api/categories/{id}");
        }

        public async Task<List<Category>?> GetItemsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Category>>("api/categories");
        }

        public async Task<bool> UpdateItemAsync(Category item)
        {
            var reponse = await _httpClient.PutAsJsonAsync($"api/categories/{item.Id}", item);
            return reponse.IsSuccessStatusCode;
        }
    }
}
