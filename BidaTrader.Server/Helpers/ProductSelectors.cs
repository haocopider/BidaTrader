using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using System.Linq.Expressions;

namespace BidaTrader.Server.Helpers
{
    public static class ProductSelectors
    {
        public static Expression<Func<Product, ProductDto>> ToDto =>
            p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                Rating = p.Rating,

                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,

                ImageUrl = p.ProductImages
                    .Where(i => i.IsMain)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),

                StoreId = p.StoreId,
                StoreName = p.Store.StoreName,
                StoreLogo = p.Store.LogoUrl,

                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
    }

}
