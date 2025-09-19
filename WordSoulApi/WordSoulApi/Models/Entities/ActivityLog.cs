using System.ComponentModel.DataAnnotations;

namespace WordSoulApi.Models.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [MaxLength(100)]
        public required string Action { get; set; } // Ví dụ: "Login", "AssignRole", "LearnVocabulary"
        [MaxLength(100)]
        public string? Details { get; set; } // Chi tiết: "User logged in at 2025-09-09"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
