using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.VocabularySet
{
    /// <summary>
    /// DTO cho endpoint POST /api/vocabulary-sets/ai-create.
    /// Admin chỉ cần cung cấp danh sách từ và thông tin bộ từ vựng;
    /// hệ thống tự động sinh metadata qua Gemini AI, ảnh Unsplash, audio Azure TTS.
    /// </summary>
    public class AiCreateVocabularySetDto
    {
        // ── Thông tin VocabularySet ──────────────────────────────
        [Required(ErrorMessage = "Title is required")]
        public required string Title { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Theme is required")]
        public VocabularySetTheme Theme { get; set; }

        public string? Description { get; set; }

        public VocabularyDifficultyLevel DifficultyLevel { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPublic { get; set; } = true;

        [Required(ErrorMessage = "At least one word is required")]
        public List<VocabularyPreviewDto> Vocabularies { get; set; } = [];
    }
}
