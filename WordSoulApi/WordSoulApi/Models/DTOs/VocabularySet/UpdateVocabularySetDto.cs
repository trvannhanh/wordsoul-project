using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.VocabularySet
{
    public class UpdateVocabularySetDto
    {
        public required string Title { get; set; }
        public VocabularySetTheme Theme { get; set; }
        public string? Description { get; set; }
        public VocabularyDifficultyLevel DifficultyLevel { get; set; }
        public bool IsActive { get; set; }
        public List<int> VocabularyIds { get; set; } = new List<int>();
    }
}
