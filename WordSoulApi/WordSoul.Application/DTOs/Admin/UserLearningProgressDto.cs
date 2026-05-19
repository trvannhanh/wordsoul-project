namespace WordSoul.Application.DTOs.Admin
{
    public class UserLearningProgressDto
    {
        public int UserId { get; set; }

        // Memory state distribution
        public int NewCount { get; set; }
        public int LearningCount { get; set; }
        public int ReviewCount { get; set; }
        public int MasteredCount { get; set; }
        public int TotalVocabularies { get; set; }

        // SRS summary
        public int DueForReviewCount { get; set; }
        public DateTime? NextReviewTime { get; set; }

        // Accuracy
        public int TotalCorrect { get; set; }
        public int TotalWrong { get; set; }
        public double AccuracyRate { get; set; }

        // Retention
        public double AverageRetentionScore { get; set; }

        // Sessions (last 30 days)
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }

        // Top struggle words
        public List<StruggleWordEntry> StruggleWords { get; set; } = [];
    }

    public class StruggleWordEntry
    {
        public string Word { get; set; } = string.Empty;
        public string? Meaning { get; set; }
        public int WrongCount { get; set; }
        public double RetentionScore { get; set; }
    }
}
