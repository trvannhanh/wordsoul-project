using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.DTOs.Notification;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IRealtimeNotificationService _realtimeService;
        private readonly IUnitOfWork _uow;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<NotificationService> _logger;
        private readonly IFcmService _fcmService;

        public NotificationService(
            INotificationRepository notificationRepository,
            IRealtimeNotificationService realtimeService,
            IUnitOfWork uow,
            IActivityLogService activityLogService,
            ILogger<NotificationService> logger,
            IFcmService fcmService)
        {
            _notificationRepository = notificationRepository;
            _realtimeService = realtimeService;
            _uow = uow;
            _activityLogService = activityLogService;
            _logger = logger;
            _fcmService = fcmService;
        }

        //------------------------------- CREATE -----------------------------------

        // Tạo mới thông báo và gửi qua SignalR
        public async Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, string? actionUrl = null, CancellationToken ct = default)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = actionUrl
            };

            await _notificationRepository.CreateNotificationAsync(notification);
            await _uow.SaveChangesAsync(ct);
            await _realtimeService.SendNotificationAsync(userId, notification);

            // Push FCM
            var user = await _uow.User.GetUserByIdAsync(userId, ct);
            if (user != null && !string.IsNullOrEmpty(user.FcmToken))
            {
                await _fcmService.SendPushNotificationAsync(user.FcmToken, title, message, actionUrl);
            }
        }

        // Broadcast tới nhiều user (hoặc tất cả user nếu TargetUserIds == null)
        public async Task<BroadcastResultDto> BroadcastAsync(BroadcastNotificationDto dto, int adminUserId, CancellationToken ct = default)
        {
            List<int> userIds;

            if (dto.TargetUserIds is { Count: > 0 })
            {
                userIds = dto.TargetUserIds;
            }
            else
            {
                var allUsers = await _uow.User.GetAllUsersAsync(
                    name: null, email: null, role: null,
                    topXP: null, topAP: null,
                    pageNumber: 1, pageSize: int.MaxValue,
                    cancellationToken: ct);
                userIds = allUsers.Select(u => u.Id).ToList();
            }

            foreach (var userId in userIds)
            {
                await CreateNotificationAsync(userId, dto.Title, dto.Message, dto.Type, dto.ActionUrl, ct);
            }

            var logDetail = $"Title: \"{dto.Title}\", Type: {dto.Type}, Sent to: {userIds.Count} user(s)";
            await _activityLogService.CreateActivityLogAsync(
                userId: adminUserId,
                action: "ADMIN_BROADCAST",
                details: logDetail,
                ct: ct);

            _logger.LogInformation("Admin broadcast sent: {Details}", logDetail);

            return new BroadcastResultDto
            {
                NotificationsSent = userIds.Count,
                BroadcastAt = DateTime.UtcNow,
            };
        }

        //-------------------------------------READ-------------------------------------------
        // Lấy tất cả thông báo của người dùng
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
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });

        }

        //-------------------------------------UPDATE-----------------------------------------

        // Đánh dấu thông báo đã đọc
        public async Task MarkAsReadNotificationAsync(int id, CancellationToken ct = default)
        {
            await _notificationRepository.MarkAsReadNotificationAsync(id);
            await _uow.SaveChangesAsync(ct);
        }
        // Đánh dấu tất cả thông báo của người dùng đã đọc
        public async Task MarkAllAsReadAsync(int userId, CancellationToken ct = default)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
            await _uow.SaveChangesAsync(ct);
        }

        //-------------------------------------DELETE-----------------------------------------
        // Xóa thông báo
        public async Task DeleteNotificationAsync(int id, int currentUserId, CancellationToken ct = default)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null || notification.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You cannot delete this notification.");
            }
            await _notificationRepository.DeleteNotificationAsync(id);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
