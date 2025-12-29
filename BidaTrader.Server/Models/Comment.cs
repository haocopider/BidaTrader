using System;
using System.Collections.Generic;

namespace BidaTrader.Server.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int AccountId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
