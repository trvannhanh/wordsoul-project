using System.ComponentModel.DataAnnotations;

namespace WordSoulApi.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string? Username { get; set; }
        [MaxLength(100)]
        public required string Email { get; set; }
        [MaxLength(200)]
        public required string PasswordHash { get; set; }
        public int XP { get; set; } = 0; // Experience Points
        public int AP { get; set; } = 0; // Achivement Points
        public UserRole Role { get; set; } = UserRole.User; // Default role is User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        [MaxLength(200)]
        public string? RefreshToken { get; set; } 
        public DateTime? RefreshTokenExpiryTime { get; set; }


        public List<VocabularySet> CreatedVocabularySets { get; set; } = new List<VocabularySet>();
        public List<LearningSession> LearningSessions { get; set; } = new List<LearningSession>(); 
        public List<UserVocabularySet> UserVocabularySets { get; set; } = new List<UserVocabularySet>(); 
        public List<UserVocabularyProgress> UserVocabularyProgresses { get; set; } = new List<UserVocabularyProgress>();
        public List<UserOwnedPet> UserOwnedPets { get; set; } = new List<UserOwnedPet>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();


    }

    public enum UserRole
    {
        User, 
        Admin 
    }
}
