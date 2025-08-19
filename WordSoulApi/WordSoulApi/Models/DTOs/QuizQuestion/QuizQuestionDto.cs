using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.QuizQuestion
{
    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public required string Prompt { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public required string CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
        public int VocabularyId { get; set; }
    }

    public class AdminQuizQuestionDto : QuizQuestionDto
    {
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
