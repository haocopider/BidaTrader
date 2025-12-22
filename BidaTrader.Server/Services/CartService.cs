using Azure.Core;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class CartService : ServerService<Cart>
    {
        public CartService(AppDbContext context) : base(context) { }

        public async Task<List<CartGroupDto>> MyCart(int accountId)
        {
            var cartItems = await _context.Carts
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Store)
                .Where(ci => ci.AccountId == accountId)
                .ToListAsync();

                var groupedCart = await _context.Carts
                    .Where(ci => ci.AccountId == accountId)
                    .GroupBy(ci => new
                    {
                        ci.Product.StoreId,
                        ci.Product.Store.StoreName
                    })
                    .Select(g => new CartGroupDto
                    {
                        StoreId = g.Key.StoreId,
                        StoreName = g.Key.StoreName,
                        Items = g.Select(ci => new CartItemDto
                        {
                            ProductId = ci.ProductId,
                            ProductName = ci.Product.Name,
                            ProductImage = ci.Product.ProductImages
                                .Where(img => img.IsMain)
                                .Select(img => img.ImageUrl)
                                .FirstOrDefault(),
                            Price = ci.Product.Price,
                            Quantity = ci.Quantity,
                            Stock = ci.Product.Quantity,
                            StoreId = ci.Product.StoreId
                        }).ToList()
                    })
                    .ToListAsync();

            return groupedCart;

        }

        public async Task AddToCart(int accountId,Cart p)
        {
            var existingItem = await _context.Carts
                .FirstOrDefaultAsync(ci => ci.AccountId == accountId && ci.ProductId == p.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += p.Quantity;
            }
            else
            {
                var newItem = new Cart
                {
                    AccountId = accountId,
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(newItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantity(int accountId, Cart c)
        {
            var item = await _context.Carts
                .FirstOrDefaultAsync(ci =>
                    ci.AccountId == accountId &&
                    ci.ProductId == c.ProductId);

            if (item == null)
                throw new Exception("Sản phẩm không tồn tại trong giỏ hàng");

            if (c.Quantity <= 0)
            {
                _context.Carts.Remove(item);
            }
            else
            {
                item.Quantity = c.Quantity;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductInCart(int accountId, int productId)
        {
            var item = await _context.Carts
    .FirstOrDefaultAsync(ci => ci.AccountId == accountId && ci.ProductId == productId);

            if (item != null)
            {
                _context.Carts.Remove(item);
            }
            await _context.SaveChangesAsync();
        }

        public async Task Checkout(int accountId, CheckoutRequestDto crd)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var productIds = crd.SelectedItems.Select(x => x.ProductId).ToList();

                var productsInDb = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Include(p => p.Store)
                    .ToListAsync();

                foreach (var item in crd.SelectedItems)
                {
                    var product = productsInDb.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null)
                        throw new Exception("Sản phẩm không tồn tại");

                    if (product.Quantity < item.Quantity)
                        throw new Exception($"'{product.Name}' không đủ tồn kho");
                }

                var ordersByStore = productsInDb.GroupBy(p => p.StoreId);

                foreach (var storeGroup in ordersByStore)
                {
                    decimal storeTotal = 0;
                    var orderDetails = new List<OrderDetail>();

                    foreach (var product in storeGroup)
                    {
                        var qty = crd.SelectedItems
                            .First(x => x.ProductId == product.Id).Quantity;

                        product.Quantity -= qty;

                        orderDetails.Add(new OrderDetail
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Quantity = qty,
                            PriceAtPurchase = product.Price
                        });

                        storeTotal += product.Price * qty;
                    }

                    var order = new Order
                    {
                        AccountId = accountId,
                        StoreId = storeGroup.Key,
                        OrderDate = DateTime.Now,
                        TotalAmount = storeTotal,
                        Status = "Pending",
                        ConfirmedAt = null,
                        PaymentMethod = crd.PaymentMethod,
                        IsPaid = false,
                        Note = string.IsNullOrWhiteSpace(crd.Note) ? null : crd.Note,
                        ShippingAddress = crd.ShippingAddress,
                        PhoneNumber = crd.PhoneNumber,
                        OrderDetails = orderDetails
                    };

                    _context.Orders.Add(order);
                }

                var cartItems = await _context.Carts
                    .Where(ci => ci.AccountId == accountId &&
                                 productIds.Contains(ci.ProductId))
                    .ToListAsync();

                _context.Carts.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
