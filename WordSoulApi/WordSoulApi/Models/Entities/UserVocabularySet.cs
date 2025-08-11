namespace WordSoulApi.Models.Entities
{
    public class UserVocabularySet
    {
        public int UserId { get; set; } // Foreign key to User
        public User User { get; set; } // Navigation property to User
        public int VocabularySetId { get; set; } // Foreign key to VocabularySet
        public VocabularySet VocabularySet { get; set; } // Navigation property to VocabularySet
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp of when the vocabulary set was created for the user
        public bool IsActive { get; set; } = true; // Indicates if the vocabulary set is currently active for the user
    }
}
