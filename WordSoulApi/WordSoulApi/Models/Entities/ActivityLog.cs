namespace WordSoulApi.Models.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Action { get; set; } // Ví dụ: "Login", "AssignRole", "LearnVocabulary"
        public string Details { get; set; } // Chi tiết: "User logged in at 2025-09-09"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
