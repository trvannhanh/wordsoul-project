using WordSoul.Application.DTOs.User;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IUserVocabularySetService
    {
        // Thêm bộ từ vựng vào người dùng
        Task AddVocabularySetToUserAsync(int userId, int vocabId);
        // Lấy tất cả bộ từ vựng của người dùng
        Task<UserVocabularySetDto> GetUserVocabularySetAsync(int userId, int vocabSetId);
    }
}