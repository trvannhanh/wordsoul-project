namespace WordSoul.Application.DTOs.Admin
{
    public class UserReviewHistoryItemDto
    {
        public int ReviewId { get; set; }
        public int VocabularyId { get; set; }
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string QuestionType { get; set; } = "";
        public DateTime ReviewTime { get; set; }
        public bool IsCorrect { get; set; }
        public double ResponseTimeSeconds { get; set; }
        public int HintCount { get; set; }
        public int Grade { get; set; }
        public double EaseFactorBefore { get; set; }
        public double EaseFactorAfter { get; set; }
        public int IntervalBefore { get; set; }
        public int IntervalAfter { get; set; }
        public DateTime? NextReviewBefore { get; set; }
        public DateTime? NextReviewAfter { get; set; }
        public string? Notes { get; set; }
    }

    public class UserReviewHistoryPageDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<UserReviewHistoryItemDto> Items { get; set; } = [];
        // Summary stats (computed over the full filtered set)
        public double AccuracyPercent { get; set; }
        public double AvgGrade { get; set; }
        public double AvgResponseTimeSeconds { get; set; }
    }
}
