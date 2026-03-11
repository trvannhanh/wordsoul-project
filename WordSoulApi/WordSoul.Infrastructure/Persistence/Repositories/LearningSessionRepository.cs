using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class LearningSessionRepository : ILearningSessionRepository
    {
        private readonly WordSoulDbContext _context;

        public LearningSessionRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // -------------------------------------CREATE-----------------------------------------
        // Tạo một LearningSession mới
        public async Task<LearningSession> CreateLearningSessionAsync(LearningSession learningSession, CancellationToken cancellationToken = default)
        {
            await _context.LearningSessions.AddAsync(learningSession, cancellationToken);
            return learningSession;
        }

        // -------------------------------------READ-------------------------------------------
        // Lấy LearningSession theo ID
        public async Task<LearningSession?> GetLearningSessionByIdAsync(int sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.LearningSessions
                .AsNoTracking() // Tối ưu hiệu suất khi chỉ đọc dữ liệu
                .FirstOrDefaultAsync(ls => ls.Id == sessionId, cancellationToken);
        }

        // Lấy LearningSession chưa hoàn thành tồn tại của User với bộ từ vựng cụ thể
        public async Task<LearningSession?> GetExistingLearningSessionUnCompletedForUserAsync(int userId, int vocabularySetId, CancellationToken cancellationToken = default)
        {
            return await _context.LearningSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(ls => ls.UserId == userId && ls.VocabularySetId == vocabularySetId && !ls.IsCompleted && ls.Type == SessionType.Learning, cancellationToken);
        }

        // Lấy ReviewSession chưa hoàn thành tồn tại của User với bộ từ vựng cụ thể
        public async Task<LearningSession?> GetExistingReviewSessionUnCompletedForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.LearningSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(ls => ls.UserId == userId && ls.Type == SessionType.Review && !ls.IsCompleted, cancellationToken);
        }

        public async Task<LearningSession?> GetExistingLearningSessionForUserAsync(int userId, int sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.LearningSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(ls => ls.Id == sessionId && ls.UserId == userId, cancellationToken);
        }

        //-------------------------------------UPDATE-----------------------------------------
        // Cập nhật một LearningSession hiện có
        public async Task<LearningSession> UpdateLearningSessionAsync(LearningSession learningSession, CancellationToken cancellationToken = default)
        {
            _context.LearningSessions.Update(learningSession);
            return await Task.FromResult(learningSession);
        }

        // -------------------------------------OTHER------------------------------------------
        // Kiểm tra LearningSession thuộc về User
        public async Task<bool> CheckUserLearningSessionExist(int userId, int sessionId, CancellationToken cancellationToken = default)
        {
            // Sử dụng AsNoTracking để tối ưu hiệu suất khi chỉ đọc dữ liệu
            return await _context.LearningSessions
                .AsNoTracking()
                .AnyAsync(ls => ls.Id == sessionId && ls.UserId == userId, cancellationToken);
        }

        // Lấy Top N chủ đề yêu thích của người dùng dựa trên số phiên học đã hoàn thành
        public async Task<List<(VocabularySetTheme Theme, int Count)>> GetUserFavoriteThemesAsync(
            int userId,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var rawResults = await _context.LearningSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId
                         && s.IsCompleted
                         && s.VocabularySetId != null
                         && s.VocabularySet != null)
                .Include(s => s.VocabularySet)
                .GroupBy(s => s.VocabularySet!.Theme)
                .Select(g => new { Theme = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return rawResults
                .Select(x => (x.Theme, x.Count))
                .ToList();
        }
    }
}