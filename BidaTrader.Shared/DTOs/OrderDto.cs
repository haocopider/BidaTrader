using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.DTOs
{

    public class OrderDto
    {
        public int Id { get; set; }

        // Thông tin chung
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string Note { get; set; }

        // Thanh toán & giao hàng
        public string PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }

        // Shop
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreAvatar { get; set; }

        // Customer (cho Store xem)
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAvatar { get; set; }

        // 🔥 SẢN PHẨM TRONG ĐƠN
        public List<OrderItemDto> Items { get; set; } = new();

        // ===== Helper UI =====
        public string? FirstImage { get; set; }

        public string ProductNamesSummary
        {
            get
            {
                if (Items == null || !Items.Any()) return "";
                var names = Items.Select(x => x.ProductName).Take(2);
                var summary = string.Join(", ", names);
                if (Items.Count > 2)
                    summary += $" và {Items.Count - 2} sản phẩm khác";
                return summary;
            }
        }
    }

    public class OrderDetailDto
    {
        public int Id { get; set; }

        // Thông tin đơn
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }

        // Thanh toán
        public string PaymentMethod { get; set; }
        public bool IsPaid { get; set; }

        // Giao hàng
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }

        // Tiền
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }

        // Shop
        public int StoreId { get; set; }
        public string StoreName { get; set; }

        // Sản phẩm
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class StoreOrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }

        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
    }

}
