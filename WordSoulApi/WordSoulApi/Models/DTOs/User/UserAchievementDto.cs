namespace WordSoulApi.Models.DTOs.User
{
    public class UserAchievementDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ConditionType { get; set; }
        public int ConditionValue { get; set; }
        public string? ItemName { get; set; }
        public string? ItemImageUrl { get; set; }
        public int ProgressValue { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; } = DateTime.Now;
    }
}
