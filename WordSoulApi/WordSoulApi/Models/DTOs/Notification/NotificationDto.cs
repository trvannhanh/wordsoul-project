using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; } // Chỉ giữ UserId, không giữ toàn bộ User
        public string Title { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
