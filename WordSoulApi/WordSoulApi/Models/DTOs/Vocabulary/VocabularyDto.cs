using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.Vocabulary
{
    public class VocabularyDto
    {
        public int Id { get; set; }
        public required string Word { get; set; }
        public required string Meaning { get; set; }
        public string? Pronunciation { get; set; } // e.g., "/wɜːrd/"
        public string? PartOfSpeech { get; set; }
        public string? CEFRLevel { get; set; } // Common European Framework of Reference for Languages level
        public string? Description { get; set; } // e.g., "A word used to describe something", "An action or state of being"
        public string? ExampleSentence { get; set; }
        public string? ImageUrl { get; set; }
        public string? PronunciationUrl { get; set; }

    }
}
