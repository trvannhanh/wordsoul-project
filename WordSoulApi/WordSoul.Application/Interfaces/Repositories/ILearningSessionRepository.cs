using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface ILearningSessionRepository
    {
        // ----------------------------- CREATE -----------------------------
        Task<LearningSession> CreateLearningSessionAsync(
            LearningSession learningSession,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        Task<LearningSession?> GetLearningSessionByIdAsync(
            int sessionId,
            CancellationToken cancellationToken = default);

        Task<LearningSession?> GetExistingLearningSessionUnCompletedForUserAsync(
            int userId,
            int vocabularySetId,
            CancellationToken cancellationToken = default);

        Task<LearningSession?> GetExistingReviewSessionUnCompletedForUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<LearningSession?> GetExistingLearningSessionForUserAsync(
            int userId,
            int sessionId,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        Task<LearningSession> UpdateLearningSessionAsync(
            LearningSession learningSession,
            CancellationToken cancellationToken = default);

        // ----------------------------- OTHER -------------------------------
        Task<bool> CheckUserLearningSessionExist(
            int userId,
            int sessionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Phân tích chuỗi phiên học đã hoàn thành của người dùng để tìm ra
        /// những chủ đề (Theme) được học nhiều nhất.
        /// </summary>
        /// <param name="userId">ID người dùng cần phân tích.</param>
        /// <param name="limit">Số chủ đề top trả về (mặc định 5).</param>
        /// <returns>Danh sách tuple (Theme, SốPhiênHoànThành), sắp xếp giảm dần.</returns>
        Task<List<(VocabularySetTheme Theme, int Count)>> GetUserFavoriteThemesAsync(
            int userId,
            int limit = 5,
            CancellationToken cancellationToken = default);
    }
}