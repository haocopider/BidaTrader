using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class OrderService : ServerService<Order>
    {
        public OrderService(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrders(int accountId, string? status)
        {
            var query = _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Account)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductImages)
                .Where(o => o.AccountId == accountId);

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => new OrderDto
            {
                // ===== Thông tin chung =====
                Id = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                Note = o.Note,

                // ===== Thanh toán & giao hàng =====
                PaymentMethod = o.PaymentMethod,
                IsPaid = o.IsPaid,
                ShippingAddress = o.ShippingAddress,
                PhoneNumber = o.PhoneNumber,

                // ===== Store =====
                StoreId = o.StoreId,
                StoreName = o.Store.StoreName,
                StoreAvatar = o.Store.LogoUrl,

                // ===== Customer =====
                CustomerId = o.AccountId,
                CustomerName = o.Account.LastName,
                CustomerAvatar = o.Account.AvatarUrl,

                FirstImage = o.OrderDetails
                    .SelectMany(od => od.Product.ProductImages)
                    .FirstOrDefault(img => img.IsMain)?.ImageUrl,

                // ===== Items =====
                Items = o.OrderDetails.Select(od => new OrderItemDto
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.PriceAtPurchase,
                }).ToList(),
            } );
        }

        public async Task<IEnumerable<StoreOrderDto>> GetStoreOrders(int accountId)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (store == null)
                throw new Exception("Bạn chưa có cửa hàng.");

            return await _context.Orders
                .Where(o => o.StoreId == store.Id)
                .Select(o => new StoreOrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    CustomerName = o.Account.FirstName + " " + o.Account.LastName,
                    PhoneNumber = o.PhoneNumber
                })
                .ToListAsync();
        }

        public async Task UpdateOrderStatus(
            int accountId,
            int orderId,
            string newStatus,
            bool isAdmin)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new Exception("Đơn hàng không tồn tại.");

            if (!isAdmin)
            {
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.AccountId == accountId);

                if (store == null || store.Id != order.StoreId)
                    throw new UnauthorizedAccessException();
            }

            order.Status = newStatus;

            await _context.SaveChangesAsync();
        }

        public async Task<OrderDetailDto?> GetOrderDetail(int orderId, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.Store)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.AccountId == userId);

            if (order == null)
                return null;

            return new OrderDetailDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.OrderDetails.Select(od => new OrderItemDto
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.PriceAtPurchase,
                    ImageUrl = od.Product.ProductImages
                        .FirstOrDefault(i => i.IsMain)?.ImageUrl
                }).ToList()
            };
        }
    }
}
