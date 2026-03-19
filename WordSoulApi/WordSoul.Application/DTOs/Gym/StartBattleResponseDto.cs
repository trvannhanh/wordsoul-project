using WordSoul.Application.DTOs.QuizQuestion;

namespace WordSoul.Application.DTOs.Gym
{
    /// <summary>
    /// Trả về khi bắt đầu một GymBattle. Chứa thông tin session và danh sách câu hỏi.
    /// </summary>
    public class StartBattleResponseDto
    {
        public int BattleSessionId { get; set; }
        public int GymLeaderId { get; set; }
        public string GymLeaderName { get; set; } = "";
        public string GymLeaderTitle { get; set; } = "";
        public string? GymLeaderAvatarUrl { get; set; }
        public int TotalQuestions { get; set; }
        public int PassRatePercent { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = [];
    }
}
