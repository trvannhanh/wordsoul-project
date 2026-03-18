using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Đại diện cho một Gym Leader trong hệ thống Gym Leader Progression.
    /// Mỗi GymLeader gắn với một VocabularySetTheme và một cấp độ CEFR.
    /// Dữ liệu được seed tĩnh, không thay đổi trong runtime.
    /// </summary>
    public class GymLeader
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }           // e.g. "Norm"

        [MaxLength(200)]
        public required string Title { get; set; }          // e.g. "Guardian of Daily Words"

        [MaxLength(500)]
        public required string Description { get; set; }    // Flavor text hiện trên UI

        [MaxLength(300)]
        public string? AvatarUrl { get; set; }              // Artwork URL

        [MaxLength(100)]
        public required string BadgeName { get; set; }      // e.g. "Boulder Badge"

        [MaxLength(300)]
        public string? BadgeImageUrl { get; set; }          // Badge image URL

        // FK đến Achievement — khi người dùng defeat Gym này, cấp Achievement tương ứng
        public int? BadgeAchievementId { get; set; }
        public Achievement? BadgeAchievement { get; set; }

        // Mapping chủ đề từ vựng (1:1 với VocabularySetTheme hiện có)
        public VocabularySetTheme Theme { get; set; }

        // Cấp độ CEFR tối thiểu của từ cần đếm để mở khóa
        public CEFRLevel RequiredCefrLevel { get; set; }

        // Thứ tự thách đấu (1–8 cho Kanto)
        public int GymOrder { get; set; }

        // Điều kiện mở khóa
        public int XpThreshold { get; set; }              // Tổng XP của user phải đạt
        public int VocabThreshold { get; set; }           // Số từ ≥ RequiredMemoryState phải đạt

        // Trạng thái MemoryState tối thiểu để đếm từ ("Learning" hoặc "Review")
        [MaxLength(20)]
        public string RequiredMemoryState { get; set; } = "Learning";

        // Phần thưởng
        public int XpReward { get; set; }

        // Cấu hình battle
        public int QuestionCount { get; set; } = 15;       // Số câu hỏi
        public int PassRatePercent { get; set; } = 80;     // % đúng tối thiểu để pass
        public int CooldownHours { get; set; } = 12;       // Cooldown (giờ) sau khi thua

        // Navigation
        public List<UserGymProgress> UserGymProgresses { get; set; } = [];
        public List<BattleSession> BattleSessions { get; set; } = [];
    }
}
