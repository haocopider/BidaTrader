namespace BidaTrader.Shared.DTOs
{
    public class CheckoutRequestDto
    {
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string? Note { get; set; }

        public List<CheckoutItemDto> SelectedItems { get; set; }
    }

    public class CheckoutResultDto
    {
        public int OrderId { get; set; }
        public long TotalAmount { get; set; }

    }

    public class CheckoutItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}