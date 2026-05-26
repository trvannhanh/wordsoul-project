namespace WordSoul.Application.DTOs.VocabularySet
{
    public class VocabularySetFullDetailDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Theme { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? DifficultyLevel { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public int? CreatedById { get; set; }           // Dùng để kiểm tra quyền owner ở frontend
        public string? CreatedByUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VocabularyDetailDto> Vocabularies { get; set; } = [];
        public int TotalVocabularies { get; set; } // Total number of vocabularies in the set
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalVocabularies / PageSize);
    }

    // Reusable DTO for Vocabulary details — bao gồm cả Override fields
    public class VocabularyDetailDto
    {
        public int Id { get; set; }
        public string? Word { get; set; }
        public string? Meaning { get; set; }           // Override nếu có, fallback về Vocabulary.Meaning
        public string? ImageUrl { get; set; }
        public string? Pronunciation { get; set; }     // Override nếu có
        public string? PronunciationUrl { get; set; }  // URL âm thanh phát âm từ Azure Speech
        public string? ExampleSentenceAudioUrl { get; set; } // URL âm thanh câu ví dụ từ Azure Speech
        public string? PartOfSpeech { get; set; }
        public string? ExampleSentence { get; set; }   // Override nếu có
        public string? Description { get; set; }       // Override nếu có

        // Metadata cho frontend biết từ này có được chỉnh sửa trong bộ không
        public bool IsCustomEdited { get; set; }
        public string? OriginalMeaning { get; set; }   // Nghĩa gốc từ Vocabulary (để hiển thị so sánh)
    }
}
