using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.DTOs.UserVocabularyProgress;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserVocabularyProgressService
    {
        // Cập nhật tiến trình học từ vựng của người dùng trong một phiên học cụ thể
        //Task<UpdateProgressResponseDto> UpdateProgressAsync(int userId, int sessionId, int vocabId);
        Task<UserProgressDto> GetUserProgressAsync(int userId);
    }
}