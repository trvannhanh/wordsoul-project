using WordSoulApi.Models.DTOs.LearningSession;

namespace WordSoulApi.Services.Interfaces
{
    public interface ILearningSessionService
    {
        Task<CompleteSessionResponseDto> CompleteSessionAsync(int userId, int sessionId);

        // Tạo một phiên học mới cho người dùng
        Task<LearningSessionDto> CreateLearningSessionAsync(int userId, int setId, int wordCount = 5);
    }
}