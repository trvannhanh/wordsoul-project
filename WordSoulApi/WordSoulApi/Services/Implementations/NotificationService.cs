using Microsoft.AspNetCore.SignalR;
using WordSoulApi.Hubs;
using WordSoulApi.Models.DTOs.Notification;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationRepository notificationRepository, IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Type = n.Type.ToString(),
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });

        }

        public async Task CreateNotificationAsync(int userId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type
            };

            await _notificationRepository.CreateNotificationAsync(notification);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }

        public async Task MarkAsReadNotificationAsync(int id)
        {
            await _notificationRepository.MarkAsReadNotificationAsync(id);
        }

        public async Task DeleteNotificationAsync(int id, int currentUserId)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null || notification.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You cannot delete this notification.");
            }
            await _notificationRepository.DeleteNotificationAsync(id);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }
    }
}
