using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    public class AnswerRecord
    {
        public int Id { get; set; }
        public int VocabularyId { get; set; }   // tham chiếu trực tiếp Vocabulary
        public Vocabulary? Vocabulary { get; set; }
        public int LearningSessionId { get; set; }
        public LearningSession? LearningSession { get; set; }
        public QuestionType QuestionType { get; set; } // Loại câu hỏi (Flashcard, MCQ, v.v.)
        [MaxLength(100)]
        public required string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public int AttemptCount { get; set; } = 1;
        public int HintCount { get; set; } = 0; // Số gợi ý đã sử dụng trong lần trả lời này
        public double ResponseTimeSeconds { get; set; } // Thời gian phản hồi tính bằng giây
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    

}
