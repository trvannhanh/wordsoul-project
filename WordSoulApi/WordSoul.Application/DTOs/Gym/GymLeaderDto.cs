using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.Gym
{
    /// <summary>
    /// Thông tin Gym Leader cùng với trạng thái tiến trình của user.
    /// </summary>
    public class GymLeaderDto
    {
        public int Id { get; set; }
        public int GymOrder { get; set; }
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public string BadgeName { get; set; } = "";
        public string? BadgeImageUrl { get; set; }

        public string Theme { get; set; } = "";          // VocabularySetTheme.ToString()
        public string RequiredCefrLevel { get; set; } = ""; // CEFRLevel.ToString()

        // Điều kiện mở khóa
        public int XpThreshold { get; set; }
        public int VocabThreshold { get; set; }
        public string RequiredMemoryState { get; set; } = "";

        // Battle config
        public int QuestionCount { get; set; }
        public int PassRatePercent { get; set; }
        public int XpReward { get; set; }

        // Trạng thái của user với Gym này
        public GymStatus Status { get; set; }
        public int TotalAttempts { get; set; }
        public int BestScore { get; set; }
        public DateTime? DefeatedAt { get; set; }
        public DateTime? CooldownEndsAt { get; set; }  // null = không bị cooldown
        public bool IsOnCooldown { get; set; }

        // Tiến trình hiện tại (dùng để hiển thị progress bar)
        public int CurrentXp { get; set; }
        public int CurrentVocabCount { get; set; }     // Số từ đã đủ MemoryState
    }
}
