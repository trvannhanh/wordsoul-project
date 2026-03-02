

using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    public class DailyQuest
    {
        public int Id { get; set; } // Primary key

        [MaxLength(100)]
        public required string Title { get; set; } // Title of the quest (e.g., "Learn 10 new words")

        [MaxLength(300)]
        public string? Description { get; set; } // Description of the quest

        public QuestType QuestType { get; set; }

        public int TargetValue { get; set; } // The target value to complete the quest (e.g., 10 words)

        public RewardType RewardType { get; set; }

        public int RewardValue { get; set; } //  XP/AP → dùng RewardValue

        public int? RewardReferenceId { get; set; } // Item/Pet → dùng RewardReferenceId

        public bool IsActive { get; set; } = true; // Whether the quest is currently active

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when the quest was created
    }
}