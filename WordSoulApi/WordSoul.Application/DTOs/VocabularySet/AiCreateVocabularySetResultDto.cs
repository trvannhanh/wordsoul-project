namespace WordSoul.Application.DTOs.VocabularySet
{
    /// <summary>
    /// Response trả về sau khi tạo VocabularySet bằng AI.
    /// Chứa thống kê về số từ mới/cũ/lỗi để frontend hiển thị kết quả.
    /// </summary>
    public class AiCreateVocabularySetResultDto
    {
        /// <summary>Bộ từ vựng vừa được tạo.</summary>
        public VocabularySetDto VocabularySet { get; set; } = null!;

        /// <summary>Tổng số từ trong input.</summary>
        public int TotalWords { get; set; }

        /// <summary>Số từ được AI tạo mới (chưa có trong DB).</summary>
        public int NewlyCreated { get; set; }

        /// <summary>Số từ lấy từ DB (đã tồn tại).</summary>
        public int AlreadyExisted { get; set; }

        /// <summary>Danh sách từ mà AI không xử lý được (parse JSON lỗi, API fail...).</summary>
        public List<string> FailedWords { get; set; } = [];
    }
}
