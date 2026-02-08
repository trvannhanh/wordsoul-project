

namespace WordSoul.Domain.Entities
{
    public class VocabularyReviewHistory
    {
        public int Id { get; set; } // Primary key

        public int UserId { get; set; } // Foreign key to User
        public User? User { get; set; } // Navigation property to User

        public int VocabularyId { get; set; } // Foreign key to Vocabulary
        public Vocabulary? Vocabulary { get; set; } // Navigation property to Vocabulary

        public DateTime ReviewTime { get; set; } = DateTime.UtcNow; // Timestamp of the review

        public bool IsCorrect { get; set; } // Whether the answer was correct

        public double ResponseTimeSeconds { get; set; } // Time taken to answer the question

        public int HintCount { get; set; } = 0; // Number of hints used during the review

        public string QuestionType { get; set; } = string.Empty; // Type of question (e.g., Flashcard, MCQ, Typing)

        public int Grade { get; set; } = 0; // Grade inferred from the review (0-5, based on SM-2)

        public string? Notes { get; set; } // Optional notes for additional context (e.g., "User struggled with synonyms")
    }
}