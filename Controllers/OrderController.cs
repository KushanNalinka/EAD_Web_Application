using Microsoft.AspNetCore.Mvc;
using EADWebApplication.Models;
using EADWebApplication.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using EADWebApplication.Helpers;

namespace EADWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly NotificationService _notificationService;
        private readonly ShortNotificationService _shortNotificationService;
        private readonly CancelNotificationService _cancelNotificationService;
        private readonly JwtHelper _jwtHelper;

        public OrderController(OrderService orderService,NotificationService notificationService, ShortNotificationService shortNotificationService, CancelNotificationService cancelNotificationService, JwtHelper jwtHelper)
        {
            _notificationService = notificationService;
            _cancelNotificationService = cancelNotificationService;
            _shortNotificationService = shortNotificationService;
            _orderService = orderService;
            _jwtHelper = jwtHelper;
        }
       

        // Create a new order using OrderModel
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderModel orderRequest)
        {
            var userId = User.FindFirst("UserId")?.Value;  // Get userId from token
            var email = User.FindFirst(ClaimTypes.Email)?.Value;  // Extract Email from token

            // Map OrderModel to the actual Order entity
            var order = new Order
            {
                UserId = userId,  // Set UserId from the token
                Email = email,           // Store Email in the order
                DeliveryAddress = orderRequest.DeliveryAddress,
                Note = orderRequest.Note,
                PaymentMethod = orderRequest.PaymentMethod,
                OrderItems = new List<OrderItem>(),
                OrderStatus = 0  // Default to Pending
            };

            // Calculate the total for the order and map items
            foreach (var item in orderRequest.OrderItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice,
                    VendorId = item.VendorId,
                    VendorEmail = item.VendorEmail,
                    OrderItemStatus = 0  // Default to Pending
                };

                order.OrderItems.Add(orderItem);
                order.OrderTotal += orderItem.TotalPrice;
            }

            await _orderService.CreateOrderAsync(order);
            return Ok("Order created successfully.");
        }

        // Update an order (before dispatched)
        [Authorize]
        [HttpPut("update/{orderId}")]
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] OrderModel orderUpdate)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(orderId);
            if (existingOrder == null)
            {
                return NotFound("Order not found.");
            }

            var userId = User.FindFirst("UserId")?.Value;
            if (existingOrder.UserId != userId)
            {
                return Unauthorized("You can only update your own orders.");
            }

            // Only allow update if the order is not yet dispatched
            if (existingOrder.OrderStatus != 0)
            {
                return BadRequest("Cannot update an order that is already dispatched.");
            }

            existingOrder.OrderItems = new List<OrderItem>();
            existingOrder.Note = orderUpdate.Note;
            existingOrder.DeliveryAddress = orderUpdate.DeliveryAddress;
            existingOrder.PaymentMethod = orderUpdate.PaymentMethod;

            // Recalculate the total and update items
            existingOrder.OrderTotal = 0;
            foreach (var item in orderUpdate.OrderItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice,
                    VendorId = item.VendorId,
                    VendorEmail = item.VendorEmail,
                    OrderItemStatus = 0  // Reset to Pending
                };

                existingOrder.OrderItems.Add(orderItem);
                existingOrder.OrderTotal += orderItem.TotalPrice;
            }

            await _orderService.UpdateOrderAsync(existingOrder);
            return Ok("Order updated successfully.");
        }

       

        // Get all orders by a user
        [Authorize]
        [HttpGet("user-orders")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpPost("cancel-order/{orderId}")]
        public async Task<IActionResult> CancelOrder(string orderId, [FromBody] CancelOrderRequest request)
        {
            try
            {
                await _orderService.CancelOrderAsync(orderId, request.Note);  // Use the Note from the JSON object
                return Ok("Order canceled successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);  // Return 400 Bad Request with the error message
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while canceling the order.");
            }
        }


        public class CancelOrderRequest
        {
            public string Note { get; set; }
        }



        // Request an order cancellation (Customer)
        [Authorize]
        [HttpPost("request-cancel/{orderId}")]
        public async Task<IActionResult> RequestOrderCancellation(string orderId)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            // Send cancellation request notification to CSR/Admin
            await _notificationService.CreateCancelRequestNotificationAsync(orderId, userId, email, "Cancel Request");

            return Ok("Cancellation request sent to CSR/Admin.");
        }



        // Get All Orders
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }


        // Get Order By Id
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            return Ok(order);
        }

        // Delete An Order
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(string orderId)
        {
            await _orderService.DeleteOrderAsync(orderId);
            return Ok("Order deleted successfully.");
        }

        // Get Some set of Order Details
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpGet("order-details/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(string orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            var result = new
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Email = order.Email,
                OrderStatus = order.OrderStatus
            };

            return Ok(result);
        }

       
        // Change Order Status
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpPut("change-status/{orderId}")]
        public async Task<IActionResult> ChangeOrderStatus(string orderId, [FromBody] ChangeOrderStatusRequest request)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            order.OrderStatus = request.OrderStatus;
            await _orderService.UpdateOrderAsync(order);

            return Ok("Order status updated.");
        }

        
        // Change Delivered Status and Notify Customer
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpPut("change-delivered/{orderId}")]
        public async Task<IActionResult> ChangeDeliveredStatus(string orderId, [FromBody] ChangeDeliveredStatusRequest request)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            order.Delivered = request.Delivered;
            await _orderService.UpdateOrderAsync(order);

            if (request.Delivered)
            {
                // Pass all required parameters to CreateNotificationAsync
                await _shortNotificationService.CreateNotificationAsync(
                    order.UserId,
                    order.Email,
                    "Your order has been delivered.",
                    orderId // Pass the orderId to the notification
                );
            }

            return Ok("Delivered status updated and notification sent.");
        }


        // Request Models
        public class ChangeOrderStatusRequest
        {
            public int OrderStatus { get; set; }
        }

        public class ChangeDeliveredStatusRequest
        {
            public bool Delivered { get; set; }
        }



        [Authorize(Roles = "Vendor")]
        [HttpGet("vendor-orders")]
        public async Task<IActionResult> GetVendorOrders()
        {
            var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var orders = await _orderService.GetOrdersByVendorEmailAsync(vendorEmail);

            var result = orders.Select(order => new
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Email = order.Email,
                OrderStatus = order.OrderStatus,
                Note = order.Note,
                OrderItems = order.OrderItems.Where(item => item.VendorEmail == vendorEmail).ToList()
            });

            return Ok(result);
        }

        // Endpoint for vendors to get their specific order items
        [Authorize(Roles = "Vendor")]
        [HttpGet("vendor-order-items")]
        public async Task<IActionResult> GetVendorOrderItems()
        {
            // Extract vendor email from the token
            var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(vendorEmail))
            {
                return Unauthorized("Vendor email is missing from token.");
            }

            // Get order items specific to the vendor's email
            var vendorOrderItems = await _orderService.GetOrderItemsByVendorEmailAsync(vendorEmail);

            if (vendorOrderItems == null || vendorOrderItems.Count == 0)
            {
                return NotFound("No orders found for this vendor.");
            }

            return Ok(vendorOrderItems);
        }

        // Endpoint for vendors to update the order status of a specific product in their order
        [Authorize(Roles = "Vendor")]
        [HttpPut("update-order-item-status/{orderId}/{productId}")]
        public async Task<IActionResult> UpdateOrderItemStatus(string orderId, string productId, [FromBody] UpdateOrderStatusRequest request)
        {
            // Extract vendor email from the token
            var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(vendorEmail))
            {
                return Unauthorized("Vendor email is missing from token.");
            }

            // Call the service to update the status of the product for the vendor
            var updated = await _orderService.UpdateOrderItemStatusAsync(orderId, productId, vendorEmail, request.OrderStatus);

            if (!updated)
            {
                return NotFound("Order or product not found, or you are not authorized to update this item.");
            }

            return Ok("Order item status updated successfully.");
        }

        // Define a request model for updating order status
        public class UpdateOrderStatusRequest
        {
            public int OrderStatus { get; set; }  // New status for the product
        }






    }
}

