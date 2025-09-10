namespace WordSoulApi.Models.Entities
{
    public class Vocabulary
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string Meaning { get; set; }
        public string? Pronunciation { get; set; } // e.g., "/wɜːrd/"
        public PartOfSpeech? PartOfSpeech { get; set; }
        public CEFRLevel? CEFRLevel { get; set; } // Common European Framework of Reference for Languages level
        public string? Description { get; set; } // e.g., "A word used to describe something", "An action or state of being"
        public string? ExampleSentence { get; set; }
        public string? ImageUrl { get; set; } 
        public string? PronunciationUrl { get; set; }

        public List<SetVocabulary> SetVocabularies { get; set; } = new List<SetVocabulary>(); // Collection of vocabulary sets associated with the vocabulary word
        public List<SessionVocabulary> SessionVocabularies { get; set; } = new List<SessionVocabulary>(); // Collection of learning sessions associated with the vocabulary word
        public List<AnswerRecord> AnswerRecords { get; set; } = new List<AnswerRecord>();
        public List<UserVocabularyProgress> UserVocabularyProgresses { get; set; } = new List<UserVocabularyProgress>(); // Collection of user vocabulary progress records for the vocabulary word
    }

    public enum PartOfSpeech
    {
        Noun,
        Verb,
        Adjective,
        Adverb,
        Pronoun,
        Preposition,
        Conjunction,
        Interjection
    }

    public enum CEFRLevel
    {
        A1, // Beginner
        A2, // Elementary
        B1, // Intermediate
        B2, // Upper Intermediate
        C1, // Advanced
        C2  // Proficient
    }
}
