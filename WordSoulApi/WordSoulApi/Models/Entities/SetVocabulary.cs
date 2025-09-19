namespace WordSoulApi.Models.Entities
{
    public class SetVocabulary
    {
        public int VocabularySetId { get; set; } // Foreign key to VocabularySet
        public VocabularySet? VocabularySet { get; set; } // Navigation property to VocabularySet
        public int VocabularyId { get; set; } // Foreign key to Vocabulary
        public Vocabulary? Vocabulary { get; set; } // Navigation property to Vocabulary
        public int Order { get; set; } // Order of the vocabulary in the set, useful for quizzes or learning sessions
    }
}
    