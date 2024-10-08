

using EADWebApplication.Models;
using EADWebApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;

namespace EADWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortNotificationController : ControllerBase
    {
        private readonly ShortNotificationService _notificationService;

        public ShortNotificationController(ShortNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Get all notifications (CSR/Admin only)
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationService.GetAllNotificationsAsync();
            var response = notifications.Select(n => new
            {
                n.Id,
                n.UserId,
                n.OrderId,  // Include OrderId in the response
                n.Email,
                n.Message,
                n.CreatedAt,
                n.Removed
            }).ToList();

            return Ok(response);
        }

        // Get notifications by UserId (for a specific user)
        [Authorize]
        [HttpGet("user-notifications")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var notifications = await _notificationService.GetNonRemovedNotificationsByUserIdAsync(userId);
            var response = notifications.Select(n => new
            {
                n.Id,
                n.UserId,
                n.OrderId,  // Include OrderId in the response
                n.Email,
                n.Message,
                n.CreatedAt,
                n.Removed
            }).ToList();

            return Ok(response);
        }

        // Remove Unwanted Notification when delivering
        [Authorize]
        [HttpPut("remove-notification/{notificationId}")]
        public async Task<IActionResult> RemoveNotification(string notificationId)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var notification = await _notificationService.GetNotificationByIdAsync(notificationId);

            if (notification == null)
                return NotFound("Notification not found.");

            if (notification.UserId != userId)
                return Unauthorized("You can only modify your own notifications.");

            notification.Removed = true;  // Mark the notification as removed
            await _notificationService.UpdateNotificationAsync(notification);

            return Ok("Notification marked as removed.");
        }

        [Authorize]
        [HttpPost("create-notification")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationModel model)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            await _notificationService.CreateNotificationAsync(userId, model.Email, model.Message, model.OrderId);
            return Ok("Notification created successfully.");
        }

        public class CreateNotificationModel
        {
            public string Email { get; set; }
            public string Message { get; set; }
            public string OrderId { get; set; }  // Ensure the OrderId is passed in the request body
        }

    }
}

