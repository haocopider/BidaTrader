using System;
using System.Collections.Generic;

namespace BidaTrader.Server.Models;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public decimal PriceAtPurchase { get; set; }

    public int Quantity { get; set; }

    public string ProductName { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
