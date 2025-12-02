using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Tạo một thông báo mới.
        /// </summary>
        Task CreateNotificationAsync(
            Notification notification,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        /// <summary>
        /// Lấy tất cả thông báo của một người dùng, sắp xếp theo thời gian tạo mới nhất.
        /// </summary>
        Task<List<Notification>> GetUserNotificationsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông báo theo ID.
        /// </summary>
        Task<Notification?> GetNotificationByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        /// <summary>
        /// Đánh dấu một thông báo là đã đọc.
        /// </summary>
        Task MarkAsReadNotificationAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Đánh dấu tất cả thông báo của người dùng là đã đọc.
        /// </summary>
        Task MarkAllAsReadAsync(
            int userId,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE -----------------------------
        /// <summary>
        /// Xóa một thông báo theo ID.
        /// </summary>
        Task DeleteNotificationAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}