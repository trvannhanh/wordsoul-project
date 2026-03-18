namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Ghi lại câu trả lời của từng người dùng trong một BattleSession.
    /// </summary>
    public class BattleAnswer
    {
        public int Id { get; set; }

        public int BattleSessionId { get; set; }
        public BattleSession? BattleSession { get; set; }

        // Từ vựng được hỏi
        public int VocabularyId { get; set; }
        public Vocabulary? Vocabulary { get; set; }

        // Câu trả lời của Challenger
        public string? ChallengerAnswer { get; set; }
        public bool ChallengerIsCorrect { get; set; } = false;
        public int ChallengerAnsweredMs { get; set; } = 0; // Thời gian trả lời (ms)

        // Câu trả lời của Opponent (dùng cho PvP future)
        public string? OpponentAnswer { get; set; }
        public bool OpponentIsCorrect { get; set; } = false;
        public int OpponentAnsweredMs { get; set; } = 0;

        // Thứ tự câu hỏi trong session
        public int QuestionOrder { get; set; }
    }
}
