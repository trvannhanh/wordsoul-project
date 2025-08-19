namespace WordSoulApi.Models.DTOs.User
{
    public class UserVocabularySetDto
    {
        public int UserId { get; set; } // Foreign key to User
        public int VocabularySetId { get; set; } // Foreign key to VocabularySet
    }
}
