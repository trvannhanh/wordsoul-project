using WordSoulApi.Models.DTOs.QuizQuestion;

namespace WordSoulApi.Services.Interfaces
{
    public interface IQuizQuestionService
    {
        Task<AdminQuizQuestionDto> CreateQuizQuestionAsync(CreateQuizQuestionDto quizQuestionDto);
        Task<bool> DeleteQuizQuestionAsync(int id);
        Task<IEnumerable<AdminQuizQuestionDto>> GetAllQuizQuestionsAsync();
        Task<AdminQuizQuestionDto?> GetQuizQuestionByIdAsync(int id);
        Task<AdminQuizQuestionDto> UpdateQuizQuestionAsync(int id, AdminQuizQuestionDto quizQuestionDto);
    }
}