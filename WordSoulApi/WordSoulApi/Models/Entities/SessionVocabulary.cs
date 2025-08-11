namespace WordSoulApi.Models.Entities
{
    public class SessionVocabulary
    {
        public int LearningSessionId { get; set; } // Foreign key to LearningSession
        public LearningSession LearningSession { get; set; } // Navigation property to LearningSession
        public int VocabularyId { get; set; } // Foreign key to Vocabulary
        public Vocabulary Vocabulary { get; set; } // Navigation property to Vocabulary
        public int Order { get; set; } // Order of the vocabulary in the session, useful for quizzes or learning sessions
    }
}
