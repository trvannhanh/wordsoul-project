namespace WordSoulApi.Models.DTOs.LearningSession
{
    public class CompleteSessionResponseDto
    {
        public int XpEarned { get; set; }
        public bool IsPetRewardGranted { get; set; }
        public int? PetId { get; set; } // Pet được cấp nếu có
        public string Message { get; set; }
    }
}
