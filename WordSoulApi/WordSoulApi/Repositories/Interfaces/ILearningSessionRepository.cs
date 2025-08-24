using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface ILearningSessionRepository
    {
        Task<bool> CheckUserLearningSessionExist(int userId, int sessionId);

        // Đếm số session đã hoàn thành cho một người dùng trong một bộ từ vựng
        Task<int> CountCompletedLearningSessionAsync(int userId, int vocabularySetId);
        // Tạo một LearningSession mới
        Task<LearningSession> CreateLearningSessionAsync(LearningSession learningSession);
        Task<LearningSession?> GetLearningSessionByIdAsync(int sessionId);
        Task<LearningSession> UpdateLearningSessionAsync(LearningSession learningSession);
    }
}