using System;
using System.Collections.Generic;

namespace BidaTraderShared.Data.Models;

public partial class StoreImage
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? ImageType { get; set; }

    public virtual Store Store { get; set; } = null!;
}
