using System;
using System.Collections.Generic;

namespace BidaTraderShared.Data.Models;

public partial class Post
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public bool? IsActive { get; set; }

    public bool? IsCommentEnabled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();
}
