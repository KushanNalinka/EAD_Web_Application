using MongoDB.Driver;
using EADWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace EADWebApplication.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly NotificationService _notificationService;
        private readonly CancelNotificationService _cancelNotificationService;  // Use CancelNotificationService for customer notifications
        private readonly ProductService _productService;
        public OrderService(IOptions<MongoDBSettings> mongoSettings, NotificationService notificationService, CancelNotificationService cancelNotificationService, ProductService productService)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _orders = database.GetCollection<Order>("Orders");
            _notificationService = notificationService;
            _cancelNotificationService = cancelNotificationService;  // Inject CancelNotificationService
            _productService = productService;
        }
       
        public async Task CreateOrderAsync(Order order)
        {
            foreach (var item in order.OrderItems)
            {
                // Decrease available quantity in the product
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.AvailableQuantity -= item.Quantity;
                    await _productService.UpdateProductAsync(product);
                }
            }
            await _orders.InsertOneAsync(order);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            var existingOrder = await GetOrderByIdAsync(order.Id);

            foreach (var existingItem in existingOrder.OrderItems)
            {
                var newItem = order.OrderItems.FirstOrDefault(i => i.ProductId == existingItem.ProductId);

                if (newItem != null)
                {
                    // Calculate the quantity difference
                    var quantityDifference = newItem.Quantity - existingItem.Quantity;

                    // Update product available quantity based on the difference
                    var product = await _productService.GetProductByIdAsync(existingItem.ProductId);
                    if (product != null)
                    {
                        product.AvailableQuantity -= quantityDifference;
                        await _productService.UpdateProductAsync(product);
                    }
                }
            }
            await _orders.ReplaceOneAsync(o => o.Id == order.Id, order);
        }

        public async Task DeleteOrderAsync(string orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            foreach (var item in order.OrderItems)
            {
                // Increase available quantity when the order is deleted
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.AvailableQuantity += item.Quantity;
                    await _productService.UpdateProductAsync(product);
                }
            }
            await _orders.DeleteOneAsync(o => o.Id == orderId);
        }


        // CSR/Admin cancel an order with a note and notify customer
        public async Task CancelOrderAsync(string orderId, string note)
        {
            var order = await GetOrderByIdAsync(orderId);

            // Only allow cancellation if the order has not been dispatched (status = 0)
            if (order.OrderStatus == 0)  // Order is pending, eligible for cancellation
            {
                foreach (var item in order.OrderItems)
                {
                    // Increase available quantity when the order is canceled
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.AvailableQuantity += item.Quantity;
                        await _productService.UpdateProductAsync(product);
                    }
                }


                order.OrderStatus = 3;  // Set status to Canceled
                order.Note = note;
                await UpdateOrderAsync(order);

                // Notify the customer that the order was canceled using CancelNotificationService
                await _cancelNotificationService.NotifyCustomerAsync(order.UserId, order.Email, order.Id,
                    $"Your order {orderId} has been canceled. Reason: {note}");
            }
            else if (order.OrderStatus == 1)  // Order is dispatched
            {
                // Notify the customer that the order cannot be canceled
                await _cancelNotificationService.NotifyCustomerAsync(order.UserId, order.Email, order.Id,
                    $"Your order {orderId} has already been dispatched and cannot be canceled.");
                throw new InvalidOperationException("Order has already been dispatched and cannot be canceled.");
            }
            else if (order.OrderStatus == 2)  // Order is completed
            {
                // Notify the customer that the order cannot be canceled
                await _cancelNotificationService.NotifyCustomerAsync(order.UserId, order.Email, order.Id,
                    $"Your order {orderId} has been completed and cannot be canceled.");
                throw new InvalidOperationException("Order has been completed and cannot be canceled.");
            }
            else
            {
                // Notify the customer that the order cannot be canceled due to an invalid status
                await _cancelNotificationService.NotifyCustomerAsync(order.UserId, order.Email, order.Id,
                    $"Your order {orderId} cannot be canceled due to an invalid order status.");
                throw new InvalidOperationException("Invalid order status for cancellation.");
            }
        }


        // Get an order by ID
        public async Task<Order> GetOrderByIdAsync(string orderId)
        {
            return await _orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();
        }
        // Get all orders placed by a user
        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _orders.Find(o => o.UserId == userId).ToListAsync();
        }
        // Get all orders of all users (for CSR/Admin to view and manage)
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orders.Find(_ => true).ToListAsync();
        }

       
        


        // Mark an order as delivered
        public async Task MarkOrderDeliveredAsync(string orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            order.OrderStatus = 2;  // Delivered
            await UpdateOrderAsync(order);

            // Notify the customer that the order has been delivered using CancelNotificationService
            await _cancelNotificationService.NotifyCustomerAsync(order.UserId, order.Email, order.Id, "Your order has been delivered.");
        }

       
        public async Task<List<Order>> GetOrdersByVendorEmailAsync(string vendorEmail)
        {
            var filter = Builders<Order>.Filter.ElemMatch(o => o.OrderItems, item => item.VendorEmail == vendorEmail);
            return await _orders.Find(filter).ToListAsync();
        }

        // Method to get all order items for a specific vendor's email
        public async Task<List<OrderItemResponse>> GetOrderItemsByVendorEmailAsync(string vendorEmail)
        {
            var orders = await _orders.Find(o => o.OrderItems.Any(i => i.VendorEmail == vendorEmail)).ToListAsync();

            var vendorItems = new List<OrderItemResponse>();

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.VendorEmail == vendorEmail)
                    {
                        vendorItems.Add(new OrderItemResponse
                        {
                            OrderId = order.Id,
                            UserId = order.UserId,
                            Email = order.Email,
                            OrderStatus = order.OrderStatus,
                            Note = order.Note,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.TotalPrice,
                            OrderItemStatus = item.OrderItemStatus
                        });
                    }
                }
            }

            return vendorItems;
        }

        // Define a response model to represent the vendor's order items
        public class OrderItemResponse
        {
            public string OrderId { get; set; }
            public string UserId { get; set; }
            public string Email { get; set; }
            public int OrderStatus { get; set; }
            public string Note { get; set; }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
            public int OrderItemStatus { get; set; }
        }

        // Method to update the status of a specific product (OrderItem) for a vendor
        public async Task<bool> UpdateOrderItemStatusAsync(string orderId, string productId, string vendorEmail, int newStatus)
        {
            var filter = Builders<Order>.Filter.Where(o => o.Id == orderId && o.OrderItems.Any(i => i.ProductId == productId && i.VendorEmail == vendorEmail));
            var order = await _orders.Find(filter).FirstOrDefaultAsync();

            if (order == null)
            {
                return false;  // Order or product not found for this vendor
            }

            // Find the specific product (OrderItem) in the order and update its status
            foreach (var item in order.OrderItems)
            {
                if (item.ProductId == productId && item.VendorEmail == vendorEmail)
                {
                    item.OrderItemStatus = newStatus;
                    break;
                }
            }

            // Update the order in the database with the new status for the item
            var updateResult = await _orders.ReplaceOneAsync(o => o.Id == order.Id, order);

            return updateResult.ModifiedCount > 0;
        }

    }
}

