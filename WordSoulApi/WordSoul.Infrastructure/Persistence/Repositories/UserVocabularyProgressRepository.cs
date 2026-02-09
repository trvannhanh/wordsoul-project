using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class UserVocabularyProgressRepository : IUserVocabularyProgressRepository
    {
        private readonly WordSoulDbContext _context;

        public UserVocabularyProgressRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //-------------------------------------CREATE-------------------------------------------

        // Tạo mới tiến trình học từ vựng cho người dùng
        public async Task<UserVocabularyProgress> CreateUserVocabularyProgressAsync(UserVocabularyProgress progress, CancellationToken cancellationToken = default)
        {
            await _context.UserVocabularyProgresses.AddAsync(progress, cancellationToken);
            return progress;
        }

        //-------------------------------------READ-------------------------------------------

        // Lấy tiến trình học từ vựng của người dùng theo userId và vocabularyId
        public async Task<UserVocabularyProgress?> GetUserVocabularyProgressAsync(int userId, int vocabularyId, CancellationToken cancellationToken = default)
        {
            return await _context.UserVocabularyProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.VocabularyId == vocabularyId, cancellationToken);
        }

        // Lấy danh sách từ vựng đến hạn ôn tập cho người dùng
        public async Task<List<UserVocabularyProgress>> GetDueVocabulariesAsync(int userId, DateTime asOf, CancellationToken cancellationToken = default)
        {
            return await _context.UserVocabularyProgresses
                .Where(p => p.UserId == userId && p.NextReviewTime <= asOf)
                .ToListAsync();
        }

        // Lấy tất cả tiến trình học từ vựng của người dùng
        public async Task<List<UserVocabularyProgress>> GetAllUserVocabularyProgressByUserAsync(int userId, CancellationToken ct = default)
        {
            return await _context.UserVocabularyProgresses
                .Where(p => p.UserId == userId)
                .ToListAsync(ct);
        }

        //-------------------------------------UPDATE-------------------------------------------

        // Cập nhật thông số SRS cho tiến trình học từ vựng
        public async Task<UserVocabularyProgress> UpdateSrsParametersAsync(UserVocabularyProgress progress, CancellationToken cancellationToken = default)
        {
            _context.UserVocabularyProgresses.Update(progress);
            return await Task.FromResult(progress);
        }
    }
}