using System;
using System.Collections.Generic;

namespace BidaTrader.Shared.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
