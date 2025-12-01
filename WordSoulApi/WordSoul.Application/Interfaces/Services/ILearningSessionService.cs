using WordSoul.Application.DTOs.AnswerRecord;
using WordSoul.Application.DTOs.LearningSession;
using WordSoul.Application.DTOs.QuizQuestion;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    public interface ILearningSessionService
    {
        // Hoàn thành phiên học
        Task<object> CompleteSessionAsync(int userId, int sessionId, SessionType sessionType);

        // Tạo một phiên học mới cho người dùng
        Task<LearningSessionDto> CreateLearningSessionAsync(int userId, int setId, int wordCount = 5);
        // Tạo một phiên học ôn tập mới cho người dùng
        Task<LearningSessionDto> CreateReviewingSessionAsync(int userId, int wordCount = 5);
        // Lấy các câu hỏi trong phiên học
        Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId);
        // Gửi câu trả lời cho một câu hỏi trong phiên học
        Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int userId, int sessionId, SubmitAnswerRequestDto request);
    }
}