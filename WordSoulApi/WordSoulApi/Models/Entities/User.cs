namespace WordSoulApi.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int XP { get; set; } = 0; // Experience Points
        public UserRole Role { get; set; } = UserRole.User; // Default role is User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; } 
        public DateTime? RefreshTokenExpiryTime { get; set; }


        public List<LearningSession> LearningSessions { get; set; } = new List<LearningSession>(); 
        public List<UserVocabularySet> UserVocabularySets { get; set; } = new List<UserVocabularySet>(); 
        public List<UserVocabularyProgress> UserVocabularyProgresses { get; set; } = new List<UserVocabularyProgress>();
        public List<AnswerRecord> AnswerRecords { get; set; } = new List<AnswerRecord>(); 
        public List<UserOwnedPet> UserOwnedPets { get; set; } = new List<UserOwnedPet>(); 
        
        
    }

    public enum UserRole
    {
        User, 
        Admin 
    }
}
