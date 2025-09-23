using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface ILearningSessionRepository
    {
        //-------------------------------------CREATE-----------------------------------------

        // Tạo một LearningSession mới
        Task<LearningSession> CreateLearningSessionAsync(LearningSession learningSession);
        //-------------------------------------READ-------------------------------------------
        // Lấy LearningSession chưa hoàn thành tồn tại của User với bộ từ vựng cụ thể
        Task<LearningSession?> GetExistingLearningSessionUnCompletedForUserAsync(int userId, int vocabularySetId);
        Task<LearningSession?> GetExistingLearningSessionForUserAsync(int userId, int sessionId);
        // Lấy LearningSession theo ID
        Task<LearningSession?> GetLearningSessionByIdAsync(int sessionId);
        //-------------------------------------UPDATE-----------------------------------------
        // Cập nhật một LearningSession hiện có
        Task<LearningSession> UpdateLearningSessionAsync(LearningSession learningSession);

        //-------------------------------------OTHER------------------------------------------
        // Kiểm tra LearningSession thuộc về User
        Task<bool> CheckUserLearningSessionExist(int userId, int sessionId);
    }
}