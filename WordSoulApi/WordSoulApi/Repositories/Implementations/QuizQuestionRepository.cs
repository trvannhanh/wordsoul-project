using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class QuizQuestionRepository : IQuizQuestionRepository
    {
        private readonly WordSoulDbContext _context;
        public QuizQuestionRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả các câu hỏi quiz
        public async Task<IEnumerable<QuizQuestion>> GetAllQuizQuestionsAsync()
        {
            // Using AsNoTracking for read-only operations to improve performance
            return await _context.QuizQuestions.AsNoTracking().ToListAsync();
        }

        // Lấy câu hỏi quiz theo ID, bao gồm thông tin từ vựng liên quan
        public async Task<QuizQuestion?> GetQuizQuestionByIdAsync(int id)
        {
            return await _context.QuizQuestions
                .AsNoTracking()
                .Select(q => new QuizQuestion { Id = q.Id, CorrectAnswer = q.CorrectAnswer, Explanation = q.Explanation })
                .FirstOrDefaultAsync(q => q.Id == id);
        }


        // Tạo câu hỏi quiz mới
        public async Task<QuizQuestion> CreateQuizQuestionAsync(QuizQuestion quizQuestion)
        {
            _context.QuizQuestions.Add(quizQuestion);
            await _context.SaveChangesAsync();
            return quizQuestion;
        }

        // Cập nhật câu hỏi quiz
        public async Task<QuizQuestion> UpdateQuizQuestionAsync(QuizQuestion quizQuestion)
        {
            _context.QuizQuestions.Update(quizQuestion);
            await _context.SaveChangesAsync();
            return quizQuestion;
        }

        // Xóa câu hỏi quiz theo ID
        public async Task<bool> DeleteQuizQuestionAsync(int id)
        {
            var quizQuestion = await GetQuizQuestionByIdAsync(id);
            if (quizQuestion == null) return false;
            _context.QuizQuestions.Remove(quizQuestion);
            return await _context.SaveChangesAsync() > 0;
        }


        // Lấy các câu hỏi quiz theo danh sách ID từ vựng
        public async Task<IEnumerable<QuizQuestion>> GetQuestionsByVocabularyIdsAsync(IEnumerable<int> vocabularyIds)
        {
            return await _context.QuizQuestions
                .Where(q => vocabularyIds.Contains(q.VocabularyId) && q.IsActive)
                .OrderBy(q => q.VocabularyId)
                .ThenBy(q => q.QuestionType)
                .ToListAsync();
        }


        // Lấy danh sách ID câu hỏi quiz theo ID từ vựng
        public async Task<List<int>> GetQuizIdsByVocabularyAsync(int vocabId)
        {
            return await _context.QuizQuestions
                .Where(q => q.VocabularyId == vocabId)
                .Select(q => q.Id)
                .ToListAsync();
        }

        // Kiểm tra xem người dùng đã trả lời đúng tất cả các câu hỏi liên quan đến từ vựng trong một phiên học cụ thể chưa
        public async Task<bool> CheckAllQuestionsCorrectAsync(int userId, int sessionId, int vocabId)
        {
            var quizIds = await GetQuizIdsByVocabularyAsync(vocabId);
            if (!quizIds.Any()) return false;

            var correctAnswers = await _context.AnswerRecords
                .AsNoTracking()
                .Where(a => a.UserId == userId &&
                           a.LearningSessionId == sessionId &&
                           quizIds.Contains(a.QuizQuestionId) &&
                           a.IsCorrect)
                .Select(a => a.QuizQuestionId)
                .Distinct()
                .ToListAsync();

            return quizIds.All(qid => correctAnswers.Contains(qid));
        }
    }
}
