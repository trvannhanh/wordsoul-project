namespace WordSoul.Application.DTOs.VocabularySet
{
    /// <summary>
    /// DTO trả về thống kê tiến trình học của user với một bộ từ vựng cụ thể.
    /// </summary>
    public class VocabularySetProgressDto
    {
        // ── Tổng quan bộ ─────────────────────────────────────────────────────────
        public int TotalVocabularies { get; set; }
        public int MasteredCount { get; set; }    // MemoryState = "Mastered"
        public int ReviewCount { get; set; }      // MemoryState = "Review"
        public int LearningCount { get; set; }    // MemoryState = "Learning"
        public int NewCount { get; set; }         // MemoryState = "New" (chưa học lần nào)

        // ── Chỉ số học tập ────────────────────────────────────────────────────────
        public double OverallRetentionScore { get; set; }  // Trung bình RetentionScore (0-100)
        public double CorrectRate { get; set; }            // Tỷ lệ trả lời đúng (0-100)
        public int TotalCompletedSession { get; set; }     // Số phiên học đã hoàn thành

        // ── Trạng thái bộ từ vựng ────────────────────────────────────────────────
        public DateTime? StartedAt { get; set; }           // Ngày đăng ký bộ
        public bool IsCompleted { get; set; }

        // ── Heatmap hoạt động (30 ngày gần nhất) ─────────────────────────────────
        public List<ActivityDay> ActivityHeatmap { get; set; } = [];

        // ── Top 5 từ yếu nhất ────────────────────────────────────────────────────
        public List<WeakVocabularyDto> WeakVocabularies { get; set; } = [];

        // ── Trạng thái ghi nhớ từng từ (vocabId → memoryState) ────────────────────
        public Dictionary<int, string> VocabMemoryStates { get; set; } = [];
    }

    public class ActivityDay
    {
        public DateOnly Date { get; set; }
        public int ReviewCount { get; set; }
    }

    public class WeakVocabularyDto
    {
        public int Id { get; set; }
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public decimal RetentionScore { get; set; }
        public string? MemoryState { get; set; }
    }
}
