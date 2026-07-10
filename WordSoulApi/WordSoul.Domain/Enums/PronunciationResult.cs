namespace WordSoul.Domain.Enums
{
    /// <summary>
    /// Kết quả đánh giá phát âm theo 3 mức từ Azure Pronunciation Assessment.
    /// </summary>
    public enum PronunciationResult
    {
        /// <summary>PronunciationScore >= 80: Phát âm Chuẩn → card hoàn thành ✓</summary>
        Perfect = 0,

        /// <summary>PronunciationScore 50-79: Gần đúng → xuống cuối chồng thẻ</summary>
        NearMiss = 1,

        /// <summary>PronunciationScore < 50: Sai → xuống cuối chồng, ghi nhận penalty</summary>
        Wrong = 2
    }
}
