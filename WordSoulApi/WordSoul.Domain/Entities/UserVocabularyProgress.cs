namespace WordSoul.Domain.Entities
{
    public class UserVocabularyProgress
    {
        public int UserId { get; set; } // Foreign key to User
        public User? User { get; set; } // Navigation property to User
        public int VocabularyId { get; set; } // Foreign key to Vocabulary
        public Vocabulary? Vocabulary { get; set; } // Navigation property to Vocabulary
        public int CorrectAttempt { get; set; } = 0; // Number of correct answers for the vocabulary
        public int TotalAttempt { get; set; } = 0; // Total number of answers given for the vocabulary
        public int ProficiencyLevel { get; set; } = 0; // Proficiency level for the vocabulary, can be used to track learning progress
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // Timestamp of the last update to the progress
        public DateTime NextReviewTime { get; set; }


        // SRS parameters
        public double EasinessFactor { get; set; } = 2.5; // SM-2 default
        public int Interval { get; set; } = 1; // Days until next review
        public int Repetition { get; set; } = 0; // Successful reviews in a row
        public int LastGrade { get; set; } = 0; // Last answer quality (0-5)

        // Computed property (not mapped to DB)
        public bool IsDue => NextReviewTime <= DateTime.UtcNow;
    }

}
