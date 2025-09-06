namespace WordSoulApi.Models.DTOs.LearningSession
{

    public class CompleteSessionResponseDto
    {
        public int XpEarned { get; set; }
        public string Message { get; set; }
    }

    public class CompleteLearningSessionResponseDto : CompleteSessionResponseDto
    {
        public bool IsPetRewardGranted { get; set; }
        public int? PetId { get; set; } // Pet được cấp nếu có
        public string? PetName { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? PetRarity { get; set; }
        public string? PetType { get; set; }
    }


    public class CompleteReviewingSessionResponseDto : CompleteSessionResponseDto
    {
        public int ApEarned { get; set; }
    }
}
    