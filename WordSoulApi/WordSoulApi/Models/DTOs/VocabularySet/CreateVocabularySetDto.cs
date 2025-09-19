using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.VocabularySet
{
    public class CreateVocabularySetDto
    {
        [Required(ErrorMessage = "Title is required")]
        public required string Title { get; set; }
        public IFormFile? ImageFile { get; set; }
        [Required(ErrorMessage = "Theme is required")]
        public VocabularySetTheme Theme { get; set; }
        public string? Description { get; set; }
        public VocabularyDifficultyLevel DifficultyLevel { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true; // Mới: Cho phép người dùng chọn public/private
        public List<int> VocabularyIds { get; set; } = new List<int>();
    }
}
