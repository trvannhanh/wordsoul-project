

using System.ComponentModel.DataAnnotations;

namespace WordSoul.Domain.Entities
{
    public class SessionVocabulary
    {
        public int LearningSessionId { get; set; } // Foreign key to LearningSession
        public LearningSession? LearningSession { get; set; } // Navigation property to LearningSession
        public int VocabularyId { get; set; } // Foreign key to Vocabulary
        public Vocabulary? Vocabulary { get; set; } // Navigation property to Vocabulary
        public int Order { get; set; } // Order of the vocabulary in the session, useful for quizzes or learning sessions

        // Position in the question flow, not a QuestionType enum value.
        public int CurrentStageIndex { get; set; } = 0;
        
        public bool IsCompleted { get; set; } = false;

        [ConcurrencyCheck]
        public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
    }
}
