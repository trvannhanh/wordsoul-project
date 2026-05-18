using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class VocabularyReviewHistoryRepository : IVocabularyReviewHistoryRepository
    {
        private readonly WordSoulDbContext _context;

        public VocabularyReviewHistoryRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task CreateReviewHistoryAsync(VocabularyReviewHistory reviewHistory, CancellationToken cancellationToken = default)
        {
            await _context.VocabularyReviewHistories.AddAsync(reviewHistory, cancellationToken);
        }

        public async Task<IEnumerable<VocabularyReviewHistory>> GetReviewHistoryByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.VocabularyReviewHistories
                .AsNoTracking()
                .Where(vrh => vrh.UserId == userId)
                .OrderByDescending(vrh => vrh.ReviewTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<VocabularyReviewHistory>> GetReviewHistoryByVocabularyAsync(int vocabularyId, CancellationToken cancellationToken = default)
        {
            return await _context.VocabularyReviewHistories
                .AsNoTracking()
                .Where(vrh => vrh.VocabularyId == vocabularyId)
                .OrderByDescending(vrh => vrh.ReviewTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy lịch sử ôn tập của user cho các từ trong một bộ, trong 30 ngày gần nhất.
        /// Được dùng để tạo Heatmap hoạt động học tập.
        /// </summary>
        public async Task<List<VocabularyReviewHistory>> GetReviewHistoryForVocabsAsync(
            int userId,
            List<int> vocabIds,
            DateTime since,
            CancellationToken cancellationToken = default)
        {
            return await _context.VocabularyReviewHistories
                .AsNoTracking()
                .Where(vrh => vrh.UserId == userId
                           && vocabIds.Contains(vrh.VocabularyId)
                           && vrh.ReviewTime >= since)
                .OrderBy(vrh => vrh.ReviewTime)
                .ToListAsync(cancellationToken);
        }
    }
}