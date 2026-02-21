namespace WordSoul.Application.DTOs.Achievement
{
    public class UserAchievementDto
    {
        public int AchievementId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }

        public int ProgressValue { get; set; }
        public int TargetValue { get; set; }

        public float ProgressPercent { get; set; }
        public int Remaining { get; set; }

        public bool IsCompleted { get; set; }
    }
}
