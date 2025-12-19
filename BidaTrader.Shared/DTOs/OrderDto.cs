using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.DTOs
{
    public enum OrderStatus { Pending, Processing, Shipping, Completed }
    public class OrderMock
    {
        public string Id { get; set; }
        public string StoreName { get; set; }
        public string ProductName { get; set; }
        public double Total { get; set; }
        public OrderStatus Status { get; set; }
        public string Image { get; set; }
    }
}
