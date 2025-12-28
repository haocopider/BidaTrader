namespace BidaTrader.Shared.DTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class CartGroupDto
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public bool IsSelected { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }

    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}