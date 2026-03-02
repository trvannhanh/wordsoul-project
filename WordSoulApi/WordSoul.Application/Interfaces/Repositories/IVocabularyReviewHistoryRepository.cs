using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IVocabularyReviewHistoryRepository
    {
        Task CreateReviewHistoryAsync(VocabularyReviewHistory reviewHistory, CancellationToken cancellationToken = default);
        Task<IEnumerable<VocabularyReviewHistory>> GetReviewHistoryByUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<VocabularyReviewHistory>> GetReviewHistoryByVocabularyAsync(int vocabularyId, CancellationToken cancellationToken = default);
    }
}