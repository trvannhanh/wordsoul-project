namespace WordSoulApi.Models.Entities
{
    public class LearningSession
    {
        public int Id { get; set; }
        public SessionStatus Status { get; set; }
        public SessionType Type { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } // Navigation property to User
        public int? VocabularySetId { get; set; }
        public VocabularySet? VocabularySet { get; set; } // Navigation property to VocabularySet
        public List<SessionVocabulary> SessionVocabularies { get; set; } = new List<SessionVocabulary>();
        public List<AnswerRecord> AnswerRecords { get; set; } = new List<AnswerRecord>();
    }
    public enum SessionStatus
    {
        InProgress,
        Completed,
        Failed
    }
    public enum SessionType
    {
        Learning,
        Review
    }
}
