using System;
using System.Collections.Generic;

namespace BidaTraderShared.Data.Models;

public partial class PostImage
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
