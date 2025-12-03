using System;
using System.Collections.Generic;

namespace BidaTraderShared.Data.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? StoreId { get; set; }

    public int CustomerId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<FeedbackImage> FeedbackImages { get; set; } = new List<FeedbackImage>();

    public virtual Product? Product { get; set; }

    public virtual Store? Store { get; set; }
}
