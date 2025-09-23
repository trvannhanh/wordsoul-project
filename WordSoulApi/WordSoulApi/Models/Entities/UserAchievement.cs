namespace WordSoulApi.Models.Entities
{
    public class UserAchievement
    {
        public int Id { get; set; }
        public int AchievementId { get; set; }
        public Achievement? Achievement { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int ProgressValue { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; } = DateTime.Now;
    }
}
