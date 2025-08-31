using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.Vocabulary
{
    public class CreateVocabularyDto
    {
        [Required(ErrorMessage = "Word is required")]
        public required string Word { get; set; }

        [Required(ErrorMessage = "Meaning is required")]
        public required string Meaning { get; set; }
        public string? Pronunciation { get; set; } // e.g., "/wɜːrd/"

        [Required(ErrorMessage = "Part of speech is required")]
        public PartOfSpeech PartOfSpeech { get; set; }
        public CEFRLevel CEFRLevel { get; set; } // Common European Framework of Reference for Languages level
        public string? Description { get; set; } // e.g., "A word used to describe something", "An action or state of being"
        public string? ExampleSentence { get; set; }
        public string? ImageUrl { get; set; }
        public string? PronunciationUrl { get; set; }
    }
}
