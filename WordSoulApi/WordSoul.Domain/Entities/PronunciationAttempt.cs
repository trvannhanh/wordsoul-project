using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Ghi lại mỗi lần người dùng luyện phát âm một từ vựng.
    /// Đây là bảng stateless — không liên kết với LearningSession.
    /// </summary>
    public class PronunciationAttempt
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int VocabularyId { get; set; }
        public Vocabulary? Vocabulary { get; set; }

        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;

        // ── Scores từ Azure Pronunciation Assessment ──────────────────────────
        /// <summary>Độ chính xác của từng phoneme (0-100)</summary>
        public double AccuracyScore { get; set; }

        /// <summary>Độ trôi chảy, tự nhiên khi phát âm (0-100)</summary>
        public double FluencyScore { get; set; }

        /// <summary>Số lượng âm tiết phát âm so với tổng số (0-100)</summary>
        public double CompletenessScore { get; set; }

        /// <summary>Score tổng hợp từ Azure (0-100), dùng để quyết định PronunciationResult</summary>
        public double PronunciationScore { get; set; }

        // ── Kết quả đánh giá ─────────────────────────────────────────────────
        /// <summary>Perfect (>=80), NearMiss (50-79), Wrong (<50)</summary>
        public PronunciationResult Result { get; set; }

        /// <summary>Dữ liệu JSON raw từ Azure để phục vụ debug và analytics</summary>
        public string? AzureRawResponse { get; set; }

        /// <summary>Số lần Perfect liên tiếp tại thời điểm attempt này (snapshot)</summary>
        public int ConsecutivePerfectCount { get; set; } = 0;

        /// <summary>XP thực tế người dùng nhận được (đã nhân PetXpMultiplier)</summary>
        public int XpAwarded { get; set; } = 0;
    }
}
