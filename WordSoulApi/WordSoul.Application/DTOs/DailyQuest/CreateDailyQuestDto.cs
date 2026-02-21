using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.DailyQuest
{
    public class CreateDailyQuestDto
    {
        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public QuestType QuestType { get; set; }
        public int TargetValue { get; set; }
        public RewardType RewardType { get; set; }
        public int RewardValue { get; set; }
        public int? RewardReferenceId { get; set; }
    }
}
