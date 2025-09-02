using WordSoulApi.Models.DTOs.AnswerRecord;
using WordSoulApi.Models.DTOs.LearningSession;
using WordSoulApi.Models.DTOs.QuizQuestion;

namespace WordSoulApi.Services.Interfaces
{
    public interface ILearningSessionService
    {
        Task<CompleteSessionResponseDto> CompleteSessionAsync(int userId, int sessionId);

        // Tạo một phiên học mới cho người dùng
        Task<LearningSessionDto> CreateLearningSessionAsync(int userId, int setId, int wordCount = 5);
        Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId);
        Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int userId, int sessionId, SubmitAnswerRequestDto request);
    }
}