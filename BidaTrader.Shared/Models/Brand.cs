using System;
using System.Collections.Generic;

namespace BidaTrader.Shared.Models;

public partial class Brand
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? OwnerStoreId { get; set; }

    public bool IsRecycled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Store? OwnerStore { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
