namespace WordSoulApi.Models.DTOs.UserVocabularyProgress
{
    public class UpdateProgressResponseDto
    {
        public int VocabularyId { get; set; }
        public int ProficiencyLevel { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime NextReviewTime { get; set; }
    }
}
