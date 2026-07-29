

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

        // Captured once from the first unaided recall in a review flow.
        // Null for learning sessions and legacy review sessions.
        public int? InitialRecallAnswerRecordId { get; set; }
        public AnswerRecord? InitialRecallAnswerRecord { get; set; }
        public DateTime? InitialRecallAt { get; set; }
        public bool? InitialRecallCorrect { get; set; }
        public int? InitialRecallGrade { get; set; }
        
        public bool IsCompleted { get; set; } = false;

        [ConcurrencyCheck]
        public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
    }
}
