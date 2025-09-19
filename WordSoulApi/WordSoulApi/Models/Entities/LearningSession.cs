namespace WordSoulApi.Models.Entities
{
    public class LearningSession
    {
        public int Id { get; set; }
        public SessionType Type { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; } = DateTime.UtcNow.AddHours(1); 
        public bool IsCompleted { get; set; } = false;
        public int UserId { get; set; }
        public User? User { get; set; } // Navigation property to User
        public int? VocabularySetId { get; set; }
        public VocabularySet? VocabularySet { get; set; } // Navigation property to VocabularySet
        public int? PetId { get; set; } // Optional reference to a Pet
        public Pet? Pet { get; set; } // Navigation property to Pet
        public List<SessionVocabulary> SessionVocabularies { get; set; } = new List<SessionVocabulary>();
        public List<AnswerRecord> AnswerRecords { get; set; } = new List<AnswerRecord>();
    }
    public enum SessionType
    {
        Learning,
        Review
    }
}
