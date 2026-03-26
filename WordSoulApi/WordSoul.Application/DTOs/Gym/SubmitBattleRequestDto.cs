namespace WordSoul.Application.DTOs.Gym
{
    /// <summary>
    /// Chứa câu trả lời của người dùng cho một câu hỏi trong BattleSession.
    /// </summary>
    public class BattleAnswerDto
    {
        public int VocabularyId { get; set; }
        public string Answer { get; set; } = "";
        public int QuestionOrder { get; set; }
        public int ResponseTimeMs { get; set; } = 0;
    }

    /// <summary>
    /// Request body khi submit toàn bộ kết quả battle.
    /// </summary>
    public class SubmitBattleRequestDto
    {
        public List<BattleAnswerDto> Answers { get; set; } = [];
    }
}
