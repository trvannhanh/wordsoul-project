using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    public class VocabularySet
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Title { get; set; } // e.g., "Daily Vocabulary", "Advanced English"
        public VocabularySetTheme Theme { get; set; }
        [MaxLength(300)]
        public string? Description { get; set; }
        [MaxLength(200)]
        public string? ImageUrl { get; set; }
        public VocabularyDifficultyLevel DifficultyLevel { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public bool IsActive { get; set; } = true;

        public int? CreatedById { get; set; }  // Foreign key đến User (nullable nếu set hệ thống)
        public User? CreatedBy { get; set; }

        public bool IsPublic { get; set; } = true; // Indicates if the vocabulary set is public or private

        public List<SetVocabulary> SetVocabularies { get; set; } = []; // Collection of vocabulary words associated with the vocabulary set
        public List<LearningSession> LearningSessions { get; set; } = []; // Collection of learning sessions associated with the vocabulary set
        public List<UserVocabularySet> UserVocabularySets { get; set; } = []; // Collection of user vocabulary sets associated with the vocabulary set
        public List<SetRewardPet> SetRewardPets { get; set; } = []; // Collection of pet sets associated with the vocabulary set
    }

}
