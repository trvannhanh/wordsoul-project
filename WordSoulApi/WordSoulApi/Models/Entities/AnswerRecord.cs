using System.ComponentModel.DataAnnotations;

namespace WordSoulApi.Models.Entities
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum QuestionType
    {
        Flashcard,
        FillInBlank,
        MultipleChoice,
        Listening
    }

}
