using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class AnswerRecordRepository : IAnswerRecordRepository
    {
        private readonly WordSoulDbContext _context;
        public AnswerRecordRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả các bản ghi trả lời
        public async Task<IEnumerable<AnswerRecord>> GetAllAnswerRecordsAsync()
        {
            return await _context.AnswerRecords.AsNoTracking().ToListAsync();
        }

        public async Task<bool> ExistsAsync(int userId, int sessionId, int questionId)
        {
            return await _context.AnswerRecords
                .AnyAsync(ar => ar.UserId == userId &&
                              ar.LearningSessionId == sessionId &&
                              ar.QuizQuestionId == questionId);
        }

        // Lấy bản ghi trả lời theo ID, bao gồm thông tin câu hỏi liên quan
        public async Task<AnswerRecord?> GetAnswerRecordByIdAsync(int id)
        {
            return await _context.AnswerRecords
                .Include(ar => ar.QuizQuestion)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == id);
        }

        // Tạo bản ghi trả lời mới
        public async Task<AnswerRecord> CreateAnswerRecordAsync(AnswerRecord answerRecord)
        {
            _context.AnswerRecords.Add(answerRecord);
            await _context.SaveChangesAsync();
            return answerRecord;
        }

        // Cập nhật bản ghi trả lời
        public async Task<AnswerRecord> UpdateAnswerRecordAsync(AnswerRecord answerRecord)
        {
            _context.AnswerRecords.Update(answerRecord);
            await _context.SaveChangesAsync();
            return answerRecord;
        }

        // Xóa bản ghi trả lời theo ID
        public async Task<bool> DeleteAnswerRecordAsync(int id)
        {
            var answerRecord = await GetAnswerRecordByIdAsync(id);
            if (answerRecord == null) return false;
            _context.AnswerRecords.Remove(answerRecord);
            return await _context.SaveChangesAsync() > 0;
        }

        // Lấy số lần thử trả lời của người dùng cho một câu hỏi trong một phiên học cụ thể
        public async Task<int> GetAttemptCountAsync(int userId, int sessionId, int questionId)
        {
            var record = await _context.AnswerRecords
            .Where(a => a.UserId == userId && a.LearningSessionId == sessionId && a.QuizQuestionId == questionId)
            .Select(a => a.AttemptCount)
            .FirstOrDefaultAsync();
            return record;
        }
    }
}
