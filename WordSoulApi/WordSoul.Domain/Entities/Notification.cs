using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public required string Title { get; set; }
        public NotificationType Type { get; set; }
        [MaxLength(200)]
        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


}
