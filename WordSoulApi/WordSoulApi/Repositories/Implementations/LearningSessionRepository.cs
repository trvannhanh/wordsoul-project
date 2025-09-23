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

        // -------------------------------------CREATE-----------------------------------------

        // Tạo một LearningSession mới
        public async Task<LearningSession> CreateLearningSessionAsync(LearningSession learningSession)
        {
            _context.LearningSessions.Add(learningSession);
            await _context.SaveChangesAsync();
            return learningSession;
        }


        // -------------------------------------READ-------------------------------------------

        // Lấy LearningSession theo ID
        public async Task<LearningSession?> GetLearningSessionByIdAsync(int sessionId)
        {
            return await _context.LearningSessions
                .AsNoTracking() // Tối ưu hiệu suất khi chỉ đọc dữ liệu
                .FirstOrDefaultAsync(ls => ls.Id == sessionId);
        }

        // Lấy LearningSession chưa hoàn thành tồn tại của User với bộ từ vựng cụ thể
        public async Task<LearningSession?> GetExistingLearningSessionUnCompletedForUserAsync(int userId, int vocabularySetId)
        {
            return await _context.LearningSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(ls => ls.UserId == userId && ls.VocabularySetId == vocabularySetId && !ls.IsCompleted);
        }

        public async Task<LearningSession?> GetExistingLearningSessionForUserAsync(int userId, int sessionId)
        {
            return await _context.LearningSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(ls => ls.Id == sessionId && ls.UserId == userId );
        }
        

        //-------------------------------------UPDATE-----------------------------------------

        // Cập nhật một LearningSession hiện có
        public async Task<LearningSession> UpdateLearningSessionAsync(LearningSession learningSession)
        {
            _context.LearningSessions.Update(learningSession);
            await _context.SaveChangesAsync();
            return learningSession;
        }



        // -------------------------------------OTHER------------------------------------------

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
