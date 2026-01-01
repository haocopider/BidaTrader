using BidaTrader.Shared.DTOs;
using Blazored.LocalStorage;

namespace BidaTrader.Client.Services
{
    public class CartStateService
    {
        private readonly ILocalStorageService _localStorage;

        public event Action? OnChange;

        public int TotalItems { get; private set; } = 0;

        public CartStateService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            var cart = await _localStorage.GetItemAsync<List<CartItemDto>>("cart");
            TotalItems = cart?.Sum(x => x.Quantity) ?? 0;
            NotifyStateChanged();
        }


        public async Task UpdateCartCount()
        {
            var cart = await _localStorage.GetItemAsync<List<CartItemDto>>("cart");
            TotalItems = cart?.Sum(x => x.Quantity) ?? 0;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}