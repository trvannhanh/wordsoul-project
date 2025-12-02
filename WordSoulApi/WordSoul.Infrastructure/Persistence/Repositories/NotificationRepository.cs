using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly WordSoulDbContext _context;

        public NotificationRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // -------------------------------------CREATE-----------------------------------------
        // Tạo mới thông báo
        public async Task CreateNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            await _context.Notifications.AddAsync(notification, cancellationToken);
        }

        // -------------------------------------READ-------------------------------------------
        // Lấy tất cả thông báo của người dùng, sắp xếp theo thời gian tạo mới nhất
        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // Lấy thông báo theo Id
        public async Task<Notification?> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications.FindAsync([id], cancellationToken);
        }

        // -------------------------------------UPDATE-----------------------------------------
        // Đánh dấu thông báo đã đọc
        public async Task MarkAsReadNotificationAsync(int id, CancellationToken cancellationToken = default)
        {
            var notification = await _context.Notifications.FindAsync([id], cancellationToken);
            if (notification != null)
            {
                notification.IsRead = true;
            }
        }

        public async Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }
        }

        // -------------------------------------DELETE-----------------------------------------
        public async Task DeleteNotificationAsync(int id, CancellationToken cancellationToken = default)
        {
            var notification = await _context.Notifications.FindAsync([id], cancellationToken);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }
        }
    }
}