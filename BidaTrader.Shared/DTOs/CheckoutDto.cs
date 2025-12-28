namespace BidaTrader.Shared.DTOs
{
    public class CheckoutRequestDto
    {
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; } // "COD", "MOMO", "BANK"
        public string? Note { get; set; }

        public List<CheckoutItemDto> SelectedItems { get; set; }
    }

    public class CheckoutItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}