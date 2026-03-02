

namespace WordSoul.Application.DTOs.DailyQuest
{
    public class ClaimQuestRewardResponseDto
    {
        public string RewardType { get; set; } = string.Empty;
        public int RewardValue { get; set; }
        public int? RewardReferenceId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
