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

        // Lấy tất cả AnswerRecord
        public async Task<IEnumerable<AnswerRecord>> GetAllAnswerRecordsAsync()
        {
            return await _context.AnswerRecords.AsNoTracking().ToListAsync();
        }

        // Kiểm tra xem đã tồn tại AnswerRecord cho user + session + vocab + questionType chưa
        public async Task<bool> ExistsAsync(int sessionId, int vocabId, QuestionType questionType)
        {
            return await _context.AnswerRecords
                .AnyAsync(ar => ar.LearningSessionId == sessionId &&
                                ar.VocabularyId == vocabId &&
                                ar.QuestionType == questionType);
        }

        // Lấy AnswerRecord theo Id
        public async Task<AnswerRecord?> GetAnswerRecordByIdAsync(int id)
        {
            return await _context.AnswerRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == id);
        }

        public async Task<AnswerRecord?> GetAnswerRecordFromSession(int sessionId, int vocabId,QuestionType questionType)
        {
            return await _context.AnswerRecords.FirstOrDefaultAsync(ar =>
                ar.LearningSessionId == sessionId &&
                ar.VocabularyId == vocabId &&
                ar.QuestionType == questionType);
        }

        // Tạo mới AnswerRecord
        public async Task<AnswerRecord> CreateAnswerRecordAsync(AnswerRecord answerRecord)
        {
            _context.AnswerRecords.Add(answerRecord);
            await _context.SaveChangesAsync();
            return answerRecord;
        }

        // Cập nhật AnswerRecord
        public async Task<AnswerRecord> UpdateAnswerRecordAsync(AnswerRecord answerRecord)
        {
            _context.AnswerRecords.Update(answerRecord);
            await _context.SaveChangesAsync();
            return answerRecord;
        }

        // Xoá AnswerRecord
        public async Task<bool> DeleteAnswerRecordAsync(int id)
        {
            var record = await GetAnswerRecordByIdAsync(id);
            if (record == null) return false;

            _context.AnswerRecords.Remove(record);
            return await _context.SaveChangesAsync() > 0;
        }

        // Đếm số lần attempt cho 1 từ + loại câu hỏi
        public async Task<int> GetAttemptCountAsync(int sessionId, int vocabId, QuestionType questionType)
        {
            var record = await _context.AnswerRecords
                .Where(a => a.LearningSessionId == sessionId &&
                            a.VocabularyId == vocabId &&
                            a.QuestionType == questionType)
                .Select(a => a.AttemptCount)
                .FirstOrDefaultAsync();

            return record;
        }

        // Kiểm tra user đã trả lời đúng tất cả các loại câu hỏi của 1 từ chưa
        public async Task<bool> CheckAllQuestionsCorrectAsync(int sessionId, int vocabId)
        {
            var totalTypes = Enum.GetValues<QuestionType>().Length;

            var correctTypes = await _context.AnswerRecords
                .AsNoTracking()
                .Where(a => a.LearningSessionId == sessionId &&
                            a.VocabularyId == vocabId &&
                            a.IsCorrect)
                .Select(a => a.QuestionType)
                .Distinct()
                .CountAsync();

            return correctTypes == totalTypes;
        }

        // Lấy danh sách các câu hỏi sai trong phiên học
        public async Task<IEnumerable<AnswerRecord>> GetWrongAnswersAsync(int sessionId)
        {
            return await _context.AnswerRecords
                .AsNoTracking()
                .Where(a => a.LearningSessionId == sessionId && !a.IsCorrect)
                .ToListAsync();
        }

        // Lấy danh sách các loại câu hỏi đã trả lời đúng cho một từ vựng trong phiên học
        public async Task<List<QuestionType>> GetCorrectAnswerTypesAsync(int sessionId, int vocabId)
        {
            return await _context.AnswerRecords
                .AsNoTracking()
                .Where(a => a.LearningSessionId == sessionId &&
                            a.VocabularyId == vocabId &&
                            a.IsCorrect)
                .Select(a => a.QuestionType)
                .Distinct()
                .ToListAsync();
        }
    }
}
