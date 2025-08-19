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
        public async Task<IEnumerable<QuizQuestion>> GetAllQuizQuestionsAsync()
        {
            // Using AsNoTracking for read-only operations to improve performance
            return await _context.QuizQuestions.AsNoTracking().ToListAsync();
        }

        public async Task<QuizQuestion?> GetQuizQuestionByIdAsync(int id)
        {
            // Using Include to load related Vocabulary entity
            return await _context.QuizQuestions
                .Include(q => q.Vocabulary)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<QuizQuestion> CreateQuizQuestionAsync(QuizQuestion quizQuestion)
        {
            _context.QuizQuestions.Add(quizQuestion);
            await _context.SaveChangesAsync();
            return quizQuestion;
        }
        public async Task<QuizQuestion> UpdateQuizQuestionAsync(QuizQuestion quizQuestion)
        {
            _context.QuizQuestions.Update(quizQuestion);
            await _context.SaveChangesAsync();
            return quizQuestion;
        }
        public async Task<bool> DeleteQuizQuestionAsync(int id)
        {
            var quizQuestion = await GetQuizQuestionByIdAsync(id);
            if (quizQuestion == null) return false;
            _context.QuizQuestions.Remove(quizQuestion);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
