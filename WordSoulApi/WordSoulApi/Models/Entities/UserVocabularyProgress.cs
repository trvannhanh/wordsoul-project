namespace WordSoulApi.Models.Entities
{
    public class UserVocabularyProgress
    {
        public int UserId { get; set; } // Foreign key to User
        public User User { get; set; } // Navigation property to User
        public int VocabularyId { get; set; } // Foreign key to Vocabulary
        public Vocabulary Vocabulary { get; set; } // Navigation property to Vocabulary
        public int CorrectAttempt { get; set; } = 0; // Number of correct answers for the vocabulary
        public int TotalAttempt { get; set; } = 0; // Total number of answers given for the vocabulary
        public int ProficiencyLevel { get; set; } = 0; // Proficiency level for the vocabulary, can be used to track learning progress
        public DateTime? LastUpdated { get; set; } = DateTime.UtcNow; // Timestamp of the last update to the progress
        public DateTime? NextReviewTime { get; set; }
    }

}
