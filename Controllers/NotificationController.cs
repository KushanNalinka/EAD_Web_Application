

using Microsoft.AspNetCore.Mvc;
using EADWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace EADWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Create a cancel request notification (for CSR/Admin to process)
        [Authorize]
        [HttpPost("create-cancel-request")]
        public async Task<IActionResult> CreateCancelRequestNotification([FromBody] CreateCancelRequestModel request)
        {
            await _notificationService.CreateCancelRequestNotificationAsync(request.OrderId, request.UserId, request.Email, "Cancel Request");
            return Ok("Cancellation request notification created.");
        }

        // Get all cancel request notifications (CSR/Admin only)
        [Authorize(Roles = "Customer Service Representative, Admin")]
        [HttpGet("cancel-requests")]
        public async Task<IActionResult> GetAllCancelRequests()
        {
            var notifications = await _notificationService.GetAllCancelRequestsAsync();
            return Ok(notifications);
        }

        // Model for cancellation request notification
        public class CreateCancelRequestModel
        {
            public string OrderId { get; set; }
            public string UserId { get; set; }
            public string Email { get; set; }
        }
    }
}
