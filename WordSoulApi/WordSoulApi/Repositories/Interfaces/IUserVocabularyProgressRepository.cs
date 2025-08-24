using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserVocabularyProgressRepository
    {
        // Tạo mới tiến trình học từ vựng cho người dùng
        Task<UserVocabularyProgress> CreateUserVocabularyProgressAsync(UserVocabularyProgress progress);
        // Lấy tiến trình học từ vựng của người dùng theo userId và vocabularyId
        Task<UserVocabularyProgress?> GetUserVocabularyProgressAsync(int userId, int vocabularyId);
        // Cập nhật tiến trình học từ vựng cho người dùng
        Task<UserVocabularyProgress> UpdateUserVocabularyProgressAsync(UserVocabularyProgress progress);
    }
}