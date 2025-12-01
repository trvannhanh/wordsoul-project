using WordSoul.Application.DTOs.User;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IUserVocabularyProgressService
    {
        // Cập nhật tiến trình học từ vựng của người dùng trong một phiên học cụ thể
        //Task<UpdateProgressResponseDto> UpdateProgressAsync(int userId, int sessionId, int vocabId);
        Task<UserProgressDto> GetUserProgressAsync(int userId);
    }
}