using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BidaTraderShared.Data.Models;

public partial class Product
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? Quantity { get; set; }

    public double? Rating { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public virtual Brand? Brand { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Category { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    [NotMapped]
    public virtual Store? Store { get; set; } = null!;
}
