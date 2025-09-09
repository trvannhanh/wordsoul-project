using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task CreateNotificationAsync(Notification notification);
        Task DeleteNotificationAsync(int id);
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task<List<Notification>> GetUserNotificationsAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
        Task MarkAsReadNotificationAsync(int id);
    }
}