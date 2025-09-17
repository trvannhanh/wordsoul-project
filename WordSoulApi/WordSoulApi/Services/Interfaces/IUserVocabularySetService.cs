using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserVocabularySetService
    {
        // Thêm bộ từ vựng vào người dùng
        Task AddVocabularySetToUserAsync(int userId, int vocabId);
        Task<UserVocabularySetDto> GetUserVocabularySetAsync(int userId, int vocabSetId);
    }
}