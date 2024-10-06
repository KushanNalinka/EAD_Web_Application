using Microsoft.AspNetCore.Mvc;
using EADWebApplication.Services;
using EADWebApplication.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EADWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductNotificationController : ControllerBase
    {
        private readonly ProductNotificationService _notificationService;

        public ProductNotificationController(ProductNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Get all notifications (Admin & CSR)
        [Authorize(Roles = "Admin,CSR")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationService.GetAllNotificationsAsync();
            return Ok(notifications);
        }

        // Get notification by ID
        [Authorize(Roles = "Admin,CSR")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(string id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound("Notification not found.");
            }
            return Ok(notification);
        }

        // Get notifications by Vendor email
        [Authorize(Roles = "Vendor")]
        [HttpGet("vendor")]
        public async Task<IActionResult> GetNotificationsByVendorEmail()
        {
            var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var notifications = await _notificationService.GetNotificationsByVendorEmailAsync(vendorEmail);
            return Ok(notifications);
        }

        // Mark notification as removed
        [Authorize(Roles = "Vendor")]
        [HttpPut("remove/{id}")]
        public async Task<IActionResult> MarkNotificationAsRemoved(string id, [FromBody] RemovedModel model)
        {
            await _notificationService.MarkNotificationAsRemovedAsync(id, model.Removed);
            return Ok("Notification updated successfully.");
        }

        public class RemovedModel
        {
            public bool Removed { get; set; }
        }
    }
}

