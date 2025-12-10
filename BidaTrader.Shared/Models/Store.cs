using System;
using System.Collections.Generic;

namespace BidaTrader.Shared.Models;

public partial class Store
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public string StoreName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public string? SocialLinks { get; set; }

    public bool IsActive { get; set; }

    public bool IsRecycled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
