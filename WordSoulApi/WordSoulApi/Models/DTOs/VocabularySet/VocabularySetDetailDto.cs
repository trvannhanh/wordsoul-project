using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.VocabularySet
{
    public class VocabularySetDetailDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public VocabularySetTheme Theme { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public VocabularyDifficultyLevel DifficultyLevel { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<int> VocabularyIds { get; set; } = new List<int>();
    }
}
