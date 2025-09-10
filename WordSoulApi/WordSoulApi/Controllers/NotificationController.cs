using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Extensions;
using WordSoulApi.Models.DTOs.Notification;
using WordSoulApi.Models.Entities;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        //GET: api/notifications: Lấy danh sách thông báo của người dùng 
        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetUserNotifications()
        {
            var userId = User.GetUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        //PUT: api/notifications/{notifiationId}/read: Đánh dấu thông báo đã đọc cho thông báo chỉ định
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadNotificationAsync(id);
            return NoContent();
        }

        //PUT: api/notifications/read-all: Đánh dấu thông báo đã đọc cho tất cả thông báo của người dùng
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        //DELETE: api/notifications/{notificationId}: Xóa thông báo chỉ định của người dùng
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = User.GetUserId();
            try
            {
                await _notificationService.DeleteNotificationAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch
            {
                return NotFound();
            }
        }

    }
}
