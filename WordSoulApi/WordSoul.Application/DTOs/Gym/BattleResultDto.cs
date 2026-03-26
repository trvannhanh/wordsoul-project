using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.Gym
{
    /// <summary>
    /// Kết quả của một BattleSession sau khi submit.
    /// </summary>
    public class BattleResultDto
    {
        public int BattleSessionId { get; set; }
        public int GymLeaderId { get; set; }
        public string GymLeaderName { get; set; } = "";

        public bool IsVictory { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int ScorePercent { get; set; }
        public int PassRatePercent { get; set; }

        // Phần thưởng (chỉ có khi thắng)
        public int XpEarned { get; set; }
        public bool BadgeEarned { get; set; }
        public string? BadgeName { get; set; }
        public string? BadgeImageUrl { get; set; }

        // Trạng thái cooldown (chỉ có khi thua)
        public bool IsOnCooldown { get; set; }
        public DateTime? CooldownEndsAt { get; set; }

        // Trạng thái mới của gym sau kết quả
        public GymStatus NewGymStatus { get; set; }
        public int BestScore { get; set; }

        // Chi tiết từng câu (cho màn hình review)
        public List<BattleAnswerResultDto> AnswerResults { get; set; } = [];
    }

    public class BattleAnswerResultDto
    {
        public int VocabularyId { get; set; }
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string? UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int QuestionOrder { get; set; }
    }
}
