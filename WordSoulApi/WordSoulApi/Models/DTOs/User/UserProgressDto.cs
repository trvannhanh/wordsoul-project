namespace WordSoulApi.Models.DTOs.User
{
    public class UserProgressDto
    {
        // Section Welcome / Thông báo ôn tập
        public int ReviewWordCount { get; set; }
        public DateTime? NextReviewTime { get; set; }

        // Biểu đồ số lượng từ theo cấp độ thành thạo
        public List<LevelStatDto> VocabularyStats { get; set; } = new();
    }

    public class LevelStatDto
    {
        public int Level { get; set; }
        public int Count { get; set; }
    }
}
