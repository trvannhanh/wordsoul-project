using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Đại diện cho một phiên battle (PvE Gym hoặc PvP tương lai).
    /// Thiết kế 1v1: ChallengerUser vs OpponentUser (null = bot/Gym).
    /// </summary>
    public class BattleSession
    {
        public int Id { get; set; }

        // ── Người thách đấu ──────────────────────────────
        public int ChallengerUserId { get; set; }
        public User? ChallengerUser { get; set; }

        // ── Đối thủ (null = Gym PvE bot) ─────────────────
        public int? OpponentUserId { get; set; }
        public User? OpponentUser { get; set; }

        // ── Gym liên quan (null nếu PvP thuần) ───────────
        public int? GymLeaderId { get; set; }
        public GymLeader? GymLeader { get; set; }

        // ── Loại + Trạng thái ────────────────────────────
        public BattleType Type { get; set; } = BattleType.GymBattle;
        public BattleStatus Status { get; set; } = BattleStatus.InProgress;

        // ── Timeline ─────────────────────────────────────
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // ── Kết quả ──────────────────────────────────────
        public int TotalQuestions { get; set; }
        public int ChallengerCorrect { get; set; } = 0;
        public int OpponentCorrect { get; set; } = 0;   // Dùng cho PvP (future)
        public bool? ChallengerWon { get; set; }         // null = chưa xong / hòa

        // ── Navigation ───────────────────────────────────
        public List<BattleAnswer> Answers { get; set; } = [];

        // ── Computed helpers ─────────────────────────────
        public int ChallengerScorePercent
            => TotalQuestions == 0 ? 0 : (int)Math.Round((double)ChallengerCorrect / TotalQuestions * 100);
    }
}
