using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class LearningSessionRepository : ILearningSessionRepository
    {
        private readonly WordSoulDbContext _context;
        public LearningSessionRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Tạo một LearningSession mới
        public async Task<LearningSession> CreateLearningSessionAsync(LearningSession learningSession)
        {
            _context.LearningSessions.Add(learningSession);
            await _context.SaveChangesAsync();
            return learningSession;
        }

        // Cập nhật một LearningSession hiện có
        public async Task<LearningSession> UpdateLearningSessionAsync(LearningSession learningSession)
        {
            _context.LearningSessions.Update(learningSession);
            await _context.SaveChangesAsync();
            return learningSession;
        }

        public async Task<LearningSession?> GetLearningSessionByIdAsync(int sessionId)
        {
            return await _context.LearningSessions
                .AsNoTracking() // Tối ưu hiệu suất khi chỉ đọc dữ liệu
                .FirstOrDefaultAsync(ls => ls.Id == sessionId);
        }

        

        // Kiểm tra LearningSession thuộc về User
        public async Task<bool> CheckUserLearningSessionExist(int userId, int sessionId)
        {
            // Sử dụng AsNoTracking để tối ưu hiệu suất khi chỉ đọc dữ liệu
            return await _context.LearningSessions
                .AsNoTracking()
                .AnyAsync(ls => ls.Id == sessionId && ls.UserId == userId);
        }



    }


}
