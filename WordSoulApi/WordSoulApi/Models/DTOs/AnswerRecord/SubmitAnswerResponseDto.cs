namespace WordSoulApi.Models.DTOs.AnswerRecord
{
    public class SubmitAnswerResponseDto
    {
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public int NewLevel { get; set; }
        public bool IsVocabularyCompleted { get; set; }
    }
}
