using System;
using System.Collections.Generic;

namespace BidaTrader.Shared.Models;

public partial class Post
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public bool IsCommentEnabled { get; set; }

    public bool IsRecycled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
