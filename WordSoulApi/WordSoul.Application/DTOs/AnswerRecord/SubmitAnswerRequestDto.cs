

using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.AnswerRecord
{
    public class SubmitAnswerRequestDto
    {
        public int VocabularyId { get; set; }
        public QuestionType QuestionType { get; set; }
        public string Answer { get; set; } = string.Empty;
    }

    
}
