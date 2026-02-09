namespace WordSoul.Application.DTOs.SRS
{
    public class VocabularyDueDto
    {
        public int VocabularyId { get; set; } // ID of the vocabulary
        public string? Word { get; set; } // The word itself
        public DateTime NextReviewDate { get; set; } // When the word is due for review
        public int Repetition { get; set; } // Number of successful repetitions
        public decimal RetentionScore { get; set; } // Retention score for the word
        public int DaysOverdue { get; set; } // Number of days the review is overdue
    }
}