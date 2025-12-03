using System;
using System.Collections.Generic;

namespace BidaTraderShared.Data.Models;

public partial class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string ShippingAddress { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Status { get; set; }

    public string? PaymentMethod { get; set; }

    public bool? PaymentStatus { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Store Store { get; set; } = null!;
}
