using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.VocabularySet
{
    public class UpdateVocabularySetDto
    {
        public required string Title { get; set; }
        public VocabularySetTheme Theme { get; set; }
        public string? Description { get; set; }
        public VocabularyDifficultyLevel DifficultyLevel { get; set; }
        public bool IsActive { get; set; }
        public List<int> VocabularyIds { get; set; } = [];
    }
}
