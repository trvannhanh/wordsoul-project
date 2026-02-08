namespace WordSoul.Domain.Entities
{
    public class UserAchievement
    {
        public int Id { get; set; }
        public int AchievementId { get; set; }
        public Achievement? Achievement { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int ProgressValue { get; set; } // Current progress value
        public bool IsCompleted { get; set; } // Whether the achievement is completed
        public DateTime? CompletedAt { get; set; } // When the achievement was completed

        // Helper methods to calculate progress dynamically
        public int GetRemaining(int target) => Math.Max(0, target - ProgressValue);

        public float GetProgressPercent(int target) => target == 0 ? 100 : (float)ProgressValue / target * 100;
    }
}
