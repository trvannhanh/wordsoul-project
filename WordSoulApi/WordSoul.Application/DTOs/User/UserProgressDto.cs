namespace WordSoul.Application.DTOs.User
{
    public class UserProgressDto
    {
        // Section Welcome / Thông báo ôn tập
        public int ReviewWordCount { get; set; }
        public DateTime? NextReviewTime { get; set; }

        // Biểu đồ số lượng từ theo cấp độ thành thạo
        public List<LevelStatDto> VocabularyStats { get; set; } = [];

        // Danh sách các từ vựng sai nhiều nhất
        public List<StruggleWordDto> StruggleWords { get; set; } = [];
    }

    public class StruggleWordDto
    {
        public int VocabularyId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string? Meaning { get; set; }
        public int WrongCount { get; set; }
    }

    public class LevelStatDto
    {
        public int Level { get; set; }
        public int Count { get; set; }
    }
}
