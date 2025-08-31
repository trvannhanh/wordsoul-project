namespace WordSoulApi.Models.Entities
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string? Prompt { get; set; }
        public QuestionType QuestionType { get; set; } // e.g., "multiple-choice", "true/false", etc.
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; }
        public string? Explanation { get; set; } // Optional explanation for the answer
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp for when the question was created
        public int VocabularyId { get; set; }
        public Vocabulary Vocabulary { get; set; } // Navigation property to the related vocabulary word

        public List<AnswerRecord> AnswerRecords { get; set; } = new List<AnswerRecord>(); // Collection of answer records associated with the quiz question

    }

    public enum QuestionType
    {
        MultipleChoice, // e.g., "multiple-choice"
        FillInTheBlank, // e.g., "fill-in-the-blank"
    }
}
