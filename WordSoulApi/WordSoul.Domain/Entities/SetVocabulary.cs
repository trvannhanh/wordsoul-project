

namespace WordSoul.Domain.Entities
{
    public class SetVocabulary
    {
        public int VocabularySetId { get; set; } // Foreign key to VocabularySet
        public VocabularySet? VocabularySet { get; set; } // Navigation property to VocabularySet
        public int VocabularyId { get; set; } // Foreign key to Vocabulary
        public Vocabulary? Vocabulary { get; set; } // Navigation property to Vocabulary
        public int Order { get; set; } // Order of the vocabulary in the set, useful for quizzes or learning sessions

        // ── Per-set override fields ───────────────────────────────────────────────
        // Khi owner chỉnh sửa nghĩa/ví dụ/phát âm trong bộ của mình,
        // hệ thống ghi vào đây thay vì sửa bảng Vocabulary gốc.
        // Khi render danh sách từ: ưu tiên Override nếu có, fallback về Vocabulary.
        public string? OverrideMeaning { get; set; }
        public string? OverrideExampleSentence { get; set; }
        public string? OverridePronunciation { get; set; }
        public string? OverrideDescription { get; set; }
    }
}
    