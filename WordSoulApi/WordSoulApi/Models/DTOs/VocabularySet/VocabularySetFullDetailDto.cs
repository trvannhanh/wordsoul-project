using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.VocabularySet
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
        public DateTime CreatedAt { get; set; }
        public List<VocabularyDetailDto> Vocabularies { get; set; } = new List<VocabularyDetailDto>();
        public int TotalVocabularies { get; set; } // Total number of vocabularies in the set
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalVocabularies / PageSize);
    }

    // Reusable DTO for Vocabulary details
    public class VocabularyDetailDto
    {
        public int Id { get; set; }
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public string? ImageUrl { get; set; }
        public string? Pronunciation { get; set; }
        public string? PartOfSpeech { get; set; }
    }
}
