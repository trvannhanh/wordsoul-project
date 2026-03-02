using WordSoul.Application.DTOs;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service xử lý nhật ký hoạt động của người dùng.
    /// </summary>
    public interface IActivityLogService
    {
        /// <summary>
        /// Tạo một nhật ký hoạt động mới.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="action">Hành động thực hiện.</param>
        /// <param name="details">Thông tin chi tiết.</param>
        /// <param name="ct">Token hủy.</param>
        Task CreateActivityLogAsync(
            int userId,
            string action,
            string details,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy nhật ký hoạt động của người dùng theo userId.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="pageNumber">Số trang.</param>
        /// <param name="pageSize">Số lượng mỗi trang.</param>
        /// <param name="ct">Token hủy.</param>
        Task<List<ActivityLogDto>> GetUserActivityLogsAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy tất cả nhật ký hoạt động, có thể lọc theo hành động hoặc ngày bắt đầu.
        /// </summary>
        /// <param name="action">Hành động lọc (tuỳ chọn).</param>
        /// <param name="fromDate">Ngày bắt đầu lọc (tuỳ chọn).</param>
        /// <param name="pageNumber">Số trang.</param>
        /// <param name="pageSize">Số lượng mỗi trang.</param>
        /// <param name="ct">Token hủy.</param>
        Task<List<ActivityLogDto>> GetAllActivityLogsAsync(
            string? action = null,
            DateTime? fromDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);

        Task TrackUserLoginAsync(int userId, CancellationToken ct = default);
        Task TrackUserLogoutAsync(int userId, CancellationToken ct = default);
        Task TrackUserRegisterAsync(int userId, CancellationToken ct = default);

        Task TrackStartLearningSessionAsync(int userId, int sessionId, CancellationToken ct = default);
        Task TrackFinishLearningSessionAsync(int userId, int sessionId, CancellationToken ct = default);
        Task TrackAnswerQuestionAsync(int userId, int vocabularyId, bool isCorrect, CancellationToken ct = default);
        Task TrackVocabularyReviewedAsync(int userId, int vocabularyId, CancellationToken ct = default);

        Task TrackPetUnlockedAsync(int userId, int petId, CancellationToken ct = default);
        Task TrackPetUpgradedAsync(int userId, int petId, CancellationToken ct = default);
        Task TrackRewardClaimedAsync(int userId, int rewardId, CancellationToken ct = default);
        Task TrackQuestClaimedAsync(int userId, int questId, CancellationToken ct = default);
        Task TrackAchievementUnlockedAsync(int userId, int achievementId, CancellationToken ct = default);
        Task TrackDailyStreakIncreasedAsync(int userId, int newStreakCount, CancellationToken ct = default);
        Task TrackDailyStreakBrokenAsync(int userId, int previousStreakCount, CancellationToken ct = default);
    }
}
