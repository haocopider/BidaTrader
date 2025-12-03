using System;
using System.Collections.Generic;

namespace BidaTraderShared.Data.Models;

public partial class FeedbackImage
{
    public int Id { get; set; }

    public int FeedbackId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual Feedback Feedback { get; set; } = null!;
}
