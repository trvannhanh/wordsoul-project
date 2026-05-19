using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.Admin
{
    public class BroadcastNotificationDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.Review;

        /// <summary>
        /// Danh sách userId nhận thông báo. null = gửi cho tất cả user.
        /// </summary>
        public List<int>? TargetUserIds { get; set; }
    }

    public class BroadcastResultDto
    {
        public int NotificationsSent { get; set; }
        public DateTime BroadcastAt { get; set; }
    }
}
