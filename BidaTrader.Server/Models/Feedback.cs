using System;
using System.Collections.Generic;

namespace BidaTrader.Server.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int AccountId { get; set; }

    public int OrderDetailId { get; set; }

    public int Rating { get; set; }

    public string? Content { get; set; }

    public string? Reply { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? RepliedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<FeedbackImage> FeedbackImages { get; set; } = new List<FeedbackImage>();

    public virtual OrderDetail OrderDetail { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
