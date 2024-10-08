using Microsoft.AspNetCore.Mvc;
using EADWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;

namespace EADWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CancelNotificationController : ControllerBase
    {
        private readonly CancelNotificationService _cancelNotificationService;

        public CancelNotificationController(CancelNotificationService cancelNotificationService)
        {
            _cancelNotificationService = cancelNotificationService;
        }

        // Get notifications by userId (for logged-in user)
        [Authorize]
        [HttpGet("user-notifications")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return Unauthorized("User ID is missing from token.");
            }

            var notifications = await _cancelNotificationService.GetNotificationsByUserIdAsync(userId);
            return Ok(notifications);
        }
    }
}
