namespace WordSoul.Application.DTOs.Battle
{
    // ── Inbound (từ client → hub) ─────────────────────────────────────────────

    public class StartArenaBattleRequestDto
    {
        public int GymLeaderId { get; set; }
        /// <summary>3 UserOwnedPet IDs người chơi chọn mang vào.</summary>
        public List<int> SelectedPetIds { get; set; } = [];
    }

    public class SubmitRoundAnswerDto
    {
        public int BattleSessionId { get; set; }
        public int RoundIndex { get; set; }
        public int VocabularyId { get; set; }
        public string Answer { get; set; } = "";
        public int ElapsedMs { get; set; }
    }

    // ── Outbound (server → client broadcast) ─────────────────────────────────

    public class BattleStartedDto
    {
        public int BattleSessionId { get; set; }
        public int TotalRounds { get; set; }
        public List<PetStateDto> P1Pets { get; set; } = [];  // người chơi
        public List<PetStateDto> P2Pets { get; set; } = [];  // bot/opponent
        public RoundQuestionDto FirstQuestion { get; set; } = null!;
        public OpponentInfoDto Opponent { get; set; } = null!;
    }

    public class OpponentInfoDto
    {
        public string Name { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public bool IsBot { get; set; } = true;
    }

    public class PetStateDto
    {
        public int SlotIndex { get; set; }
        public string DisplayName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string PetType { get; set; } = "Normal";
        public string? SecondaryPetType { get; set; }
        public int MaxHp { get; set; } = 100;
        public int CurrentHp { get; set; } = 100;
        public bool IsFainted { get; set; }
    }

    public class RoundQuestionDto
    {
        public int RoundIndex { get; set; }
        public int VocabularyId { get; set; }
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public string? Pronunciation { get; set; }
        public string? QuestionPrompt { get; set; }     // FillInBlank sentence hoặc null
        public string QuestionType { get; set; } = "";  // "MultipleChoice" | "FillInBlank"
        public List<string>? Options { get; set; }       // null nếu FillInBlank
        public int TimeLimitMs { get; set; } = 10000;   // 10 giây
    }

    public class RoundResultDto
    {
        public int RoundIndex { get; set; }
        public int VocabularyId { get; set; }
        public string CorrectAnswer { get; set; } = "";

        // Kết quả 2 bên
        public int P1Score { get; set; }
        public int P2Score { get; set; }
        public bool P1Correct { get; set; }
        public bool P2Correct { get; set; }
        public int P1AnswerMs { get; set; }
        public int P2AnswerMs { get; set; }
        public string? P1Answer { get; set; }
        public string? P2Answer { get; set; }

        // Damage
        public int DamageDealt { get; set; }
        public int DamagedPlayer { get; set; }  // 1=P1 bị, 2=P2 bị, 0=hòa
        public double TypeMultiplier { get; set; } = 1.0;
        public string? TypeEffectivenessText { get; set; }

        // Trạng thái mới
        public List<PetStateDto> P1Pets { get; set; } = [];
        public List<PetStateDto> P2Pets { get; set; } = [];
        public int P1TotalScore { get; set; }
        public int P2TotalScore { get; set; }
    }

    public class BattleEndedDto
    {
        public int BattleSessionId { get; set; }
        public bool P1Won { get; set; }
        public int P1TotalScore { get; set; }
        public int P2TotalScore { get; set; }
        public int P1CorrectCount { get; set; }
        public int P2CorrectCount { get; set; }
        public int TotalRounds { get; set; }
        public int XpEarned { get; set; }
        public bool BadgeEarned { get; set; }
        public string? BadgeName { get; set; }
        public string? BadgeImageUrl { get; set; }
        /// <summary>Chỉ có giá trị khi trận đấu là PvP.</summary>
        public PvpEloResultDto? EloResult { get; set; }
    }

    // ── Wrapper returned by SubmitAnswerAsync ─────────────────────────────────
    public class SubmitAnswerResultWrapper
    {
        public RoundResultDto? RoundResult { get; set; }
        public RoundQuestionDto? NextQuestion { get; set; }
        public BattleEndedDto? BattleEnded { get; set; }
    }
}
