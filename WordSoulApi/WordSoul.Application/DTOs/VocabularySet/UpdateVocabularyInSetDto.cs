namespace WordSoul.Application.DTOs.VocabularySet
{
    /// <summary>
    /// DTO để owner cập nhật override nghĩa/ví dụ/phát âm cho một từ vựng trong bộ của họ.
    /// Không thay đổi bảng Vocabulary gốc — chỉ ghi vào SetVocabulary.Override*.
    /// </summary>
    public class UpdateVocabularyInSetDto
    {
        public string? OverrideMeaning { get; set; }
        public string? OverrideExampleSentence { get; set; }
        public string? OverridePronunciation { get; set; }
        public string? OverrideDescription { get; set; }
    }
}
