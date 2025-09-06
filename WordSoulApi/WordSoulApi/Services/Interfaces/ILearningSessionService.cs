using WordSoulApi.Models.DTOs.AnswerRecord;
using WordSoulApi.Models.DTOs.LearningSession;
using WordSoulApi.Models.DTOs.QuizQuestion;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface ILearningSessionService
    {
        Task<object> CompleteSessionAsync(int userId, int sessionId, SessionType sessionType);

        // Tạo một phiên học mới cho người dùng
        Task<LearningSessionDto> CreateLearningSessionAsync(int userId, int setId, int wordCount = 5);
        Task<LearningSessionDto> CreateReviewingSessionAsync(int userId, int wordCount = 5);
        Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId);
        Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int userId, int sessionId, SubmitAnswerRequestDto request);
    }
}