using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
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
        public int HintBalance { get; set; } = 5; // Default 5 hints
        public UserRole Role { get; set; } = UserRole.User; // Default role is User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        [MaxLength(200)]
        public string? RefreshToken { get; set; } 
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // ── PvP Rating (ELO) ──────────────────────────────────
        public int PvpRating { get; set; } = 1000;
        public int PvpWins { get; set; } = 0;
        public int PvpLosses { get; set; } = 0;

        [MaxLength(300)]
        public string? AvatarUrl { get; set; }


        public List<VocabularySet> CreatedVocabularySets { get; set; } = [];
        public List<LearningSession> LearningSessions { get; set; } = []; 
        public List<UserVocabularySet> UserVocabularySets { get; set; } = []; 
        public List<UserVocabularyProgress> UserVocabularyProgresses { get; set; } = [];
        public List<UserOwnedPet> UserOwnedPets { get; set; } = [];
        public List<Notification> Notifications { get; set; } = [];
        public List<UserItem> UserItems { get; set; } = [];
        public List<UserAchievement> UserAchievements { get; set; } = [];
        public List<UserDailyQuest> UserDailyQuests { get; set; } = new();
        public List<UserGymProgress> UserGymProgresses { get; set; } = [];
    }

}
