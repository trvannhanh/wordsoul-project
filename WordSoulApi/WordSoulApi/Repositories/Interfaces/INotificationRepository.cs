using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        //-------------------------------------CREATE-----------------------------------------
        // Tạo mới thông báo
        Task CreateNotificationAsync(Notification notification);
        //-------------------------------------READ-------------------------------------------
        // Lấy tất cả thông báo của người dùng, sắp xếp theo thời gian tạo mới nhất
        Task<Notification?> GetNotificationByIdAsync(int id);
        // Lấy tất cả thông báo của người dùng, sắp xếp theo thời gian tạo mới nhất
        Task<List<Notification>> GetUserNotificationsAsync(int userId);
        //-------------------------------------UPDATE-----------------------------------------
        // Đánh dấu tất cả thông báo của người dùng đã đọc
        Task MarkAllAsReadAsync(int userId);
        // Đánh dấu thông báo đã đọc
        Task MarkAsReadNotificationAsync(int id);
        //-------------------------------------DELETE-----------------------------------------
        // Xóa thông báo
        Task DeleteNotificationAsync(int id);

    }
}