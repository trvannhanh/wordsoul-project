using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.AnswerRecord
{
    public class SubmitAnswerRequestDto
    {
        public int VocabularyId { get; set; }
        public QuestionType QuestionType { get; set; }
        public string Answer { get; set; } = string.Empty;
    }

    public class SubmitAnswerResponseDto
    {
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
    }
}
