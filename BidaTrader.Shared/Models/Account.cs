using System;
using System.Collections.Generic;

namespace BidaTrader.Shared.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Uid { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Passcode { get; set; }

    public string Role { get; set; } = null!;

    public int? StoreId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? AvatarUrl { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public bool IsActive { get; set; }

    public bool IsRecycled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual Store? Store { get; set; }

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}
