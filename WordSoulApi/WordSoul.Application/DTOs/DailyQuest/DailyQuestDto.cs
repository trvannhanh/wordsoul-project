

namespace WordSoul.Application.DTOs.DailyQuest
{
    public class DailyQuestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string QuestType { get; set; } = string.Empty;
        public int TargetValue { get; set; }
        public string RewardType { get; set; } = string.Empty;
        public int RewardValue { get; set; }
        public int? RewardReferenceId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserDailyQuestDto
    {
        public int Id { get; set; }
        public int DailyQuestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string QuestType { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int TargetValue { get; set; }
        public string RewardType { get; set; } = string.Empty;
        public int RewardValue { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime QuestDate { get; set; }
    }
}
