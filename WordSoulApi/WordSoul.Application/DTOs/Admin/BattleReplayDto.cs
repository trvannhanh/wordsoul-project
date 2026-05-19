namespace WordSoul.Application.DTOs.Admin
{
    public class BattleSessionSummaryDto
    {
        public int Id { get; set; }
        public string ChallengerUsername { get; set; } = "";
        public string? OpponentUsername { get; set; }   // null for GymBattle
        public string? GymLeaderName { get; set; }      // null for PvP
        public string Type { get; set; } = "";           // "GymBattle" | "PvP"
        public string Status { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int DurationSeconds { get; set; }
        public int TotalQuestions { get; set; }
        public int ChallengerCorrect { get; set; }
        public int OpponentCorrect { get; set; }
        public bool? ChallengerWon { get; set; }
        public string? RoomCode { get; set; }
    }

    public class BattleRoundDetailDto
    {
        public int RoundIndex { get; set; }
        public int VocabularyId { get; set; }
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public int? P1Score { get; set; }
        public int? P1AnswerMs { get; set; }
        public bool P1Correct { get; set; }
        public string? P1Answer { get; set; }
        public int? P2Score { get; set; }
        public int? P2AnswerMs { get; set; }
        public bool P2Correct { get; set; }
        public string? P2Answer { get; set; }
        public int DamageDealt { get; set; }
        public int DamagedPlayer { get; set; }   // 1=P1, 2=P2, 0=draw
        public double TypeMultiplier { get; set; }
    }

    public class BattleReplayDto : BattleSessionSummaryDto
    {
        public string ChallengerUsername2 { get; set; } = ""; // alias kept for clarity
        public List<BattleRoundDetailDto> Rounds { get; set; } = [];
    }

    public class BattleSessionPageDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<BattleSessionSummaryDto> Items { get; set; } = [];
    }
}
