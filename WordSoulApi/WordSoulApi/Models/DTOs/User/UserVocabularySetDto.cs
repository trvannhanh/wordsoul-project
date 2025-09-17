namespace WordSoulApi.Models.DTOs.User
{
    public class UserVocabularySetDto
    {
        public int VocabularySetId { get; set; }
        public int TotalCompletedSession { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
