using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.VocabularySet
{
    public class VocabularySetDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Theme { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? DifficultyLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedById { get; set; }
        public string? CreatedByUsername { get; set; }
        public bool IsPublic { get; set; }
        public bool IsOwned { get; set; } // Mới: Trạng thái sở hữu của user hiện tại
        public List<int> VocabularyIds { get; set; } = new List<int>();
    }
}
