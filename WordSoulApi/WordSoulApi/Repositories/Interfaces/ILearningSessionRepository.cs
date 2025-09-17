using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface ILearningSessionRepository
    {
        Task<bool> CheckUserLearningSessionExist(int userId, int sessionId);

        // Tạo một LearningSession mới
        Task<LearningSession> CreateLearningSessionAsync(LearningSession learningSession);
        Task<LearningSession?> GetExistingLearningSessionUnCompletedForUserAsync(int userId, int vocabularySetId);
        Task<LearningSession?> GetLearningSessionByIdAsync(int sessionId);
        Task<LearningSession> UpdateLearningSessionAsync(LearningSession learningSession);
    }
}