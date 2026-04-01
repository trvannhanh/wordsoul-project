namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Kết quả của từng round (câu hỏi) trong một Battle.
    /// Cả 2 bên submit câu trả lời → Server tính toán → ghi vào đây.
    /// </summary>
    public class BattleRound
    {
        public int Id { get; set; }

        public int BattleSessionId { get; set; }
        public BattleSession? BattleSession { get; set; }

        /// <summary>Chỉ số round (0-based).</summary>
        public int RoundIndex { get; set; }

        public int VocabularyId { get; set; }
        public Vocabulary? Vocabulary { get; set; }

        // ── Player 1 (Challenger) ─────────────────────────
        public int? P1Score { get; set; }          // null = chưa trả lời / hết giờ
        public int? P1AnswerMs { get; set; }        // thời gian phản hồi (ms)
        public bool P1Correct { get; set; }
        public string? P1Answer { get; set; }

        // ── Player 2 (Opponent / Bot) ─────────────────────
        public int? P2Score { get; set; }
        public int? P2AnswerMs { get; set; }
        public bool P2Correct { get; set; }
        public string? P2Answer { get; set; }

        // ── Kết quả round ────────────────────────────────
        /// <summary>Damage gây ra cho bên thua (≥ 0).</summary>
        public int DamageDealt { get; set; }
        /// <summary>Bên bị damage: 1 = P1, 2 = P2, 0 = hòa (không ai mất HP).</summary>
        public int DamagedPlayer { get; set; }

        /// <summary>Hệ số nhân damage dựa trên Type Effectiveness.</summary>
        public double TypeMultiplier { get; set; } = 1.0;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }
}
