using WordSoul.Domain.Entities;

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
    }
}