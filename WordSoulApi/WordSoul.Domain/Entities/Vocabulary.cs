

using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    public class Vocabulary
    {
        public int Id { get; set; }
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public string? Pronunciation { get; set; } // e.g., "/wɜːrd/"
        public PartOfSpeech? PartOfSpeech { get; set; }
        public CEFRLevel? CEFRLevel { get; set; } // Common European Framework of Reference for Languages level
        public string? Description { get; set; } // e.g., "A word used to describe something", "An action or state of being"
        public string? ExampleSentence { get; set; }
        public string? ImageUrl { get; set; } 
        public string? PronunciationUrl { get; set; }
        public string? ExampleSentenceAudioUrl { get; set; } // Audio MP3 of example sentence (Azure TTS)

        public bool IsCustom { get; set; } = false; // Đánh dấu từ vựng do người dùng tạo
        public int? CreatorId { get; set; } // Người tạo (nếu IsCustom = true)

        public List<SetVocabulary> SetVocabularies { get; set; } = []; // Collection of vocabulary sets associated with the vocabulary word
        public List<SessionVocabulary> SessionVocabularies { get; set; } = []; // Collection of learning sessions associated with the vocabulary word
        public List<AnswerRecord> AnswerRecords { get; set; } = [];
        public List<UserVocabularyProgress> UserVocabularyProgresses { get; set; } = []; // Collection of user vocabulary progress records for the vocabulary word
    }
    
}
