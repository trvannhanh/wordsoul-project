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

        //------------------------------- CREATE -----------------------------------


        // Tạo mới AnswerRecord
        public async Task<AnswerRecord> CreateAnswerRecordAsync(AnswerRecord answerRecord)
        {
            _context.AnswerRecords.Add(answerRecord);
            await _context.SaveChangesAsync();
            return answerRecord;
        }

        //------------------------------- READ -----------------------------------

        // Lấy AnswerRecord theo Id
        public async Task<AnswerRecord?> GetAnswerRecordByIdAsync(int id)
        {
            return await _context.AnswerRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == id);
        }

        // Lấy AnswerRecord cho một session, vocab và loại câu hỏi cụ thể
        public async Task<AnswerRecord?> GetAnswerRecordFromSession(int sessionId, int vocabId, QuestionType questionType)
        {
            return await _context.AnswerRecords.FirstOrDefaultAsync(ar =>
                ar.LearningSessionId == sessionId &&
                ar.VocabularyId == vocabId &&
                ar.QuestionType == questionType);
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


        //------------------------------- UPDATE -----------------------------------
        // Cập nhật AnswerRecord
        public async Task<AnswerRecord> UpdateAnswerRecordAsync(AnswerRecord answerRecord)
        {
            _context.AnswerRecords.Update(answerRecord);
            await _context.SaveChangesAsync();
            return answerRecord;
        }

        //------------------------------- DELETE -----------------------------------



        //------------------------------- OTHER -----------------------------------

    

       

    }
}
