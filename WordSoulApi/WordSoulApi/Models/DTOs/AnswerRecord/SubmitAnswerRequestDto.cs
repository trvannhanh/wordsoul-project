namespace WordSoulApi.Models.DTOs.AnswerRecord
{
    public class SubmitAnswerRequestDto
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; } = string.Empty;
    }

    public class SubmitAnswerResponseDto
    {
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int AttemptNumber { get; set; }
    }
}
