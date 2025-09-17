namespace WordSoulApi.Models.Entities
{
    public class VocabularySet
    {
        public int Id { get; set; }
        public required string Title { get; set; } // e.g., "Daily Vocabulary", "Advanced English"
        public VocabularySetTheme Theme { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public VocabularyDifficultyLevel DifficultyLevel { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public bool IsActive { get; set; } = true;

        public int? CreatedById { get; set; }  // Foreign key đến User (nullable nếu set hệ thống)
        public User? CreatedBy { get; set; }

        public bool IsPublic { get; set; } = true; // Indicates if the vocabulary set is public or private

        public List<SetVocabulary> SetVocabularies { get; set; } = new List<SetVocabulary>(); // Collection of vocabulary words associated with the vocabulary set
        public List<LearningSession> LearningSessions { get; set; } = new List<LearningSession>(); // Collection of learning sessions associated with the vocabulary set
        public List<UserVocabularySet> UserVocabularySets { get; set; } = new List<UserVocabularySet>(); // Collection of user vocabulary sets associated with the vocabulary set
        public List<SetRewardPet> SetRewardPets { get; set; } = new List<SetRewardPet>(); // Collection of pet sets associated with the vocabulary set
    }

    public enum VocabularySetTheme
    {
        DailyLearning, 
        AdvancedTopics, 
        Custom 
    }

    public enum VocabularyDifficultyLevel
    {
        Easy, 
        Medium, 
        Hard 
    }
}
