using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.VocabularySet
{
    public class VocabularySetDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string Theme { get; set; }
        public string ? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string DifficultyLevel { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
