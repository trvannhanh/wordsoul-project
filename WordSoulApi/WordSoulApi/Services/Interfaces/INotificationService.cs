using WordSoulApi.Models.DTOs.Notification;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message, NotificationType type);
        Task DeleteNotificationAsync(int id, int currentUserId);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
        Task MarkAsReadNotificationAsync(int id);
    }
}