namespace WordSoulApi.Models.DTOs.Home
{
    public class UserDashboardDto
    {
        // Section Welcome / Thông báo ôn tập
        public int ReviewWordCount { get; set; }
        public DateTime? NextReviewTime { get; set; }

        // Card Profile
        public string Username { get; set; } = string.Empty;
        public int Level { get; set; }
        public int TotalXP { get; set; }
        public int TotalAP { get; set; }
        public int StreakDays { get; set; }
        public int PetCount { get; set; }
        public string? AvatarUrl { get; set; }

        // Biểu đồ số lượng từ theo cấp độ thành thạo
        public List<LevelStatDto> VocabularyStats { get; set; } = new();
    }

    public class LevelStatDto
    {
        public int Level { get; set; }
        public int Count { get; set; }
    }
}
