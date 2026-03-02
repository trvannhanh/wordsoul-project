

using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface ISessionVocabularyRepository
    {
        // --------------------------- READ ---------------------------

        /// <summary>
        /// Lấy tất cả SessionVocabulary theo session ID, bao gồm Vocabulary (eager loaded).
        /// </summary>
        Task<IEnumerable<SessionVocabulary>> GetSessionVocabulariesBySessionIdAsync(
            int sessionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy một SessionVocabulary theo sessionId và vocabularyId.
        /// </summary>
        Task<SessionVocabulary?> GetSessionVocabularyAsync(
            int sessionId,
            int vocabularyId,
            CancellationToken cancellationToken = default);

        // --------------------------- UPDATE ---------------------------

        /// <summary>
        /// Cập nhật một SessionVocabulary.
        /// EF Core chỉ đánh dấu Modified, SaveChanges sẽ được gọi ở UnitOfWork.
        /// </summary>
        Task<SessionVocabulary?> UpdateSessionVocabularyAsync(
            SessionVocabulary sessionVocabulary,
            CancellationToken cancellationToken = default);
    }
}