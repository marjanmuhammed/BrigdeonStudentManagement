// Controllers/NotificationsController.cs
using Bridgeon.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace Bridgeon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        private int GetUserId()
        {
            // Use the exact claim type from your JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                // Fallback to the string representation of the claim type
                userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            }

            if (userIdClaim == null)
            {
                _logger.LogError("User ID claim not found in token");
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogError("Invalid user ID format: {UserId}", userIdClaim.Value);
                throw new UnauthorizedAccessException("Invalid user ID format");
            }

            return userId;
        }

        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            try
            {
                var userId = GetUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                return Ok(new { message = "Notifications fetched successfully", data = notifications });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Authorization failed in GetMyNotifications");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMyNotifications");
                return StatusCode(500, new { message = "An error occurred while fetching notifications" });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(new { message = "Unread count fetched", data = new { unreadCount = count } });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Authorization failed in GetUnreadCount");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUnreadCount");
                return StatusCode(500, new { message = "An error occurred while fetching unread count" });
            }
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok(new { message = "Notification marked as read" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkAsRead");
                return StatusCode(500, new { message = "An error occurred while marking notification as read" });
            }
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new { message = "All notifications marked as read" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Authorization failed in MarkAllAsRead");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkAllAsRead");
                return StatusCode(500, new { message = "An error occurred while marking all notifications as read" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id);
                return Ok(new { message = "Notification deleted" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteNotification");
                return StatusCode(500, new { message = "An error occurred while deleting notification" });
            }
        }
    }
}