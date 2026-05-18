using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IVocabularyReviewHistoryRepository
    {
        Task CreateReviewHistoryAsync(VocabularyReviewHistory reviewHistory, CancellationToken cancellationToken = default);
        Task<IEnumerable<VocabularyReviewHistory>> GetReviewHistoryByUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<VocabularyReviewHistory>> GetReviewHistoryByVocabularyAsync(int vocabularyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy lịch sử ôn tập của user cho danh sách từ vựng chỉ định trong khoảng thời gian nhất định.
        /// Dùng để tạo Heatmap hoạt động.
        /// </summary>
        Task<List<VocabularyReviewHistory>> GetReviewHistoryForVocabsAsync(
            int userId,
            List<int> vocabIds,
            DateTime since,
            CancellationToken cancellationToken = default);
    }
}