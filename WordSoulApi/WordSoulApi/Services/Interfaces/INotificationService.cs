using WordSoulApi.Models.DTOs.Notification;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface INotificationService
    {
        //-------------------------------------CREATE-----------------------------------------
        // Tạo mới thông báo
        Task CreateNotificationAsync(int userId, string title, string message, NotificationType type);

        //-------------------------------------READ-------------------------------------------
        // Lấy tất cả thông báo của người dùng, sắp xếp theo thời gian tạo mới nhất
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);


        //-------------------------------------UPDATE-----------------------------------------
        // Đánh dấu tất cả thông báo của người dùng đã đọc
        Task MarkAllAsReadAsync(int userId);
        // Đánh dấu thông báo đã đọc
        Task MarkAsReadNotificationAsync(int id);



        //-------------------------------------DELETE-----------------------------------------
        // Xóa thông báo
        Task DeleteNotificationAsync(int id, int currentUserId);


    }
}