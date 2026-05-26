using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.DTOs.Notification;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    public interface INotificationService
    {
        //-------------------------------------CREATE-----------------------------------------
        // Tạo mới thông báo
        Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, string? actionUrl = null, CancellationToken ct = default);

        // Broadcast thông báo tới nhiều user (hoặc tất cả)
        Task<BroadcastResultDto> BroadcastAsync(BroadcastNotificationDto dto, int adminUserId, CancellationToken ct = default);

        //-------------------------------------READ-------------------------------------------
        // Lấy tất cả thông báo của người dùng, sắp xếp theo thời gian tạo mới nhất
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);


        //-------------------------------------UPDATE-----------------------------------------
        // Đánh dấu tất cả thông báo của người dùng đã đọc
        Task MarkAllAsReadAsync(int userId, CancellationToken ct = default);
        // Đánh dấu thông báo đã đọc
        Task MarkAsReadNotificationAsync(int id, CancellationToken ct = default);



        //-------------------------------------DELETE-----------------------------------------
        // Xóa thông báo
        Task DeleteNotificationAsync(int id, int currentUserId, CancellationToken ct = default);


    }
}