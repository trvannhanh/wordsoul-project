using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IPronunciationAttemptRepository
    {
        /// <summary>Thêm một lần thử phát âm mới vào DB.</summary>
        Task AddAsync(PronunciationAttempt attempt, CancellationToken ct = default);

        /// <summary>Lấy lịch sử phát âm của user cho một từ cụ thể, giới hạn số lượng kết quả.</summary>
        Task<List<PronunciationAttempt>> GetByUserAndVocabAsync(
            int userId,
            int vocabularyId,
            int limit = 10,
            CancellationToken ct = default);

        /// <summary>Lấy tổng số lần Perfect của user (dùng cho Achievement check).</summary>
        Task<int> GetTotalPerfectCountAsync(int userId, CancellationToken ct = default);

        /// <summary>Lấy số từ khác nhau mà user đã luyện phát âm.</summary>
        Task<int> GetDistinctWordsPracticedAsync(int userId, CancellationToken ct = default);

        /// <summary>Lấy thống kê tổng quan phát âm của user.</summary>
        Task<PronunciationStatsDto> GetUserStatsAsync(int userId, CancellationToken ct = default);
    }

    /// <summary>DTO nội bộ repository, không expose ra ngoài Application layer.</summary>
    public class PronunciationStatsDto
    {
        public int TotalAttempts { get; set; }
        public int PerfectCount { get; set; }
        public int NearMissCount { get; set; }
        public int WrongCount { get; set; }
        public int DistinctWordsPracticed { get; set; }
        public double PerfectRate { get; set; }  // 0-100%
    }
}
