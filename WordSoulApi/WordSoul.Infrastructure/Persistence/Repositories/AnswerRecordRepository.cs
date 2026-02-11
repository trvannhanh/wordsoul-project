using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class AnswerRecordRepository : IAnswerRecordRepository
    {
        private readonly WordSoulDbContext _context;

        public AnswerRecordRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //------------------------------- CREATE -----------------------------------
        // Tạo mới AnswerRecord
        public async Task<AnswerRecord> CreateAnswerRecordAsync(AnswerRecord answerRecord, CancellationToken cancellationToken = default)
        {
            await _context.AnswerRecords.AddAsync(answerRecord, cancellationToken);
            return answerRecord;
        }

        //------------------------------- READ -----------------------------------
        // Lấy AnswerRecord theo Id
        public async Task<AnswerRecord?> GetAnswerRecordByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.AnswerRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
        }

        // Lấy AnswerRecord cho một session, vocab và loại câu hỏi cụ thể
        public async Task<AnswerRecord?> GetAnswerRecordFromSession(int sessionId, int vocabId, QuestionType questionType, CancellationToken cancellationToken = default)
        {
            return await _context.AnswerRecords.FirstOrDefaultAsync(ar =>
                ar.LearningSessionId == sessionId &&
                ar.VocabularyId == vocabId &&
                ar.QuestionType == questionType, cancellationToken);
        }

        // Đếm số lần attempt cho 1 từ + loại câu hỏi
        public async Task<int> GetAttemptCountAsync(int sessionId, int vocabId, QuestionType questionType, CancellationToken cancellationToken = default)
        {
            var record = await _context.AnswerRecords
                .Where(a => a.LearningSessionId == sessionId &&
                            a.VocabularyId == vocabId &&
                            a.QuestionType == questionType)
                .Select(a => a.AttemptCount)
                .FirstOrDefaultAsync(cancellationToken);
            return record;
        }

        public async Task<int> GetCorrectAnswerRecordNumberFromSession(int sessionId, CancellationToken cancellationToken = default)
        {
            var record = await _context.AnswerRecords
                .Where(a => a.LearningSessionId == sessionId && a.IsCorrect)
                .CountAsync(cancellationToken);
            return record;
        }

        public async Task<List<AnswerRecord>> GetAllAnswerRecordAttemptsForVocabInSession(
            int sessionId,
            int vocabularyId,
            CancellationToken cancellationToken = default)
        {
            return await _context.AnswerRecords
                .Where(ar => ar.LearningSessionId == sessionId
                          && ar.VocabularyId == vocabularyId)
                .OrderBy(ar => ar.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        //------------------------------- UPDATE -----------------------------------
        // Cập nhật AnswerRecord
        public async Task<AnswerRecord> UpdateAnswerRecordAsync(AnswerRecord answerRecord, CancellationToken cancellationToken = default)
        {
            _context.AnswerRecords.Update(answerRecord);
            return await Task.FromResult(answerRecord);
        }

        //------------------------------- DELETE -----------------------------------
        //------------------------------- OTHER -----------------------------------
    }
}