using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IQuizQuestionRepository
    {
        Task<QuizQuestion> CreateQuizQuestionAsync(QuizQuestion quizQuestion);
        Task<bool> DeleteQuizQuestionAsync(int id);
        Task<IEnumerable<QuizQuestion>> GetAllQuizQuestionsAsync();
        Task<QuizQuestion?> GetQuizQuestionByIdAsync(int id);
        Task<QuizQuestion> UpdateQuizQuestionAsync(QuizQuestion quizQuestion);
    }
}