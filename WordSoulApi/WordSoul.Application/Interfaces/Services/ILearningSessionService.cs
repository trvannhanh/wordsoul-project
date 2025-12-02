using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.AnswerRecord;
using WordSoul.Application.DTOs.LearningSession;
using WordSoul.Application.DTOs.QuizQuestion;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ quản lý LearningSession.
    /// </summary>
    public interface ILearningSessionService
    {
        /// <summary>
        /// Tạo phiên học mới.
        /// </summary>
        Task<LearningSessionDto> CreateLearningSessionAsync(
            int userId,
            int setId,
            int wordCount = 5,
            CancellationToken ct = default);

        /// <summary>
        /// Tạo phiên ôn tập mới.
        /// </summary>
        Task<LearningSessionDto> CreateReviewingSessionAsync(
            int userId,
            int wordCount = 5,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy danh sách câu hỏi của session.
        /// </summary>
        Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(
            int sessionId,
            CancellationToken ct = default);

        /// <summary>
        /// Hoàn thành session.
        /// </summary>
        Task<object> CompleteSessionAsync(
            int userId,
            int sessionId,
            SessionType sessionType,
            CancellationToken ct = default);

        /// <summary>
        /// Người dùng gửi câu trả lời.
        /// </summary>
        Task<SubmitAnswerResponseDto> SubmitAnswerAsync(
            int userId,
            int sessionId,
            SubmitAnswerRequestDto request,
            CancellationToken ct = default);
    }
}
