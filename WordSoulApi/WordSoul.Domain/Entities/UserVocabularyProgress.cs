namespace WordSoul.Domain.Entities
{
    public class UserVocabularyProgress
    {
        public int UserId { get; set; } // Foreign key to User
        public User? User { get; set; } // Navigation property to User
        public int VocabularyId { get; set; } // Foreign key to Vocabulary
        public Vocabulary? Vocabulary { get; set; } // Navigation property to Vocabulary
        public int CorrectAttempt { get; set; } = 0; // Thống kê mức độ chính xác tổng quát
        public int TotalAttempt { get; set; } = 0; // Total number of answers given for the vocabulary
        public int ProficiencyLevel { get; set; } = 0; // Mức độ thành thạo từ vựng (0-5) ở tầng UX, lớp diễn giải cho người dùng
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // Thời điểm ôn tập gần nhất
        public DateTime NextReviewTime { get; set; } // Thời điểm ôn tập tiếp theo, kết quả của SRS, tất cả lịch học xoay quanh giá trị này


        // SRS parameters for spaced repetition algorithm (SM-2)
        public double EasinessFactor { get; set; } = 2.5; // EF đại diện cho câu hỏi "từ này dễ hay khó với người này", mặc định 2.5, dễ -> tăng, khó -> giảm
        public int Interval { get; set; } = 1; // Số ngày tới lần ôn tập tiếp theo, được tính bằng công thức SRS dựa trên EF và Repetition
        public int Repetition { get; set; } = 0; // Số lần nhớ đúng liên tiếp, nếu quên reset về 0, là yếu tố quyết định khoảng cách ôn và mức độ thành thạo
        public int LastGrade { get; set; } = 0; // Điểm chất lượng lần ôn tập trước (0-5), là input trực tiếp cho SM-2

        // Computed property (not mapped to DB)
        public bool IsDue => NextReviewTime <= DateTime.UtcNow;
    }

}
