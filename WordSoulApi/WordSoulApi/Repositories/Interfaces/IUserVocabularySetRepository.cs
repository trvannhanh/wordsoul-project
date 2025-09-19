using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserVocabularySetRepository
    {
        // Thêm sự sở hữu một bộ từ vựng cho người dùng
        Task AddVocabularySetToUserAsync(UserVocabularySet userVocabularySet);
        // Kiểm tra người dùng có sở hữu bộ từ vựng này chưa
        Task<bool> CheckUserVocabularyExist(int userId, int vocabId);
        // Lấy bộ từ vựng của người dùng theo userId và vocabularySetId

        Task<UserVocabularySet?> GetUserVocabularySetAsync(int userId, int vocabularySetId);
        // Cập nhật người dùng sở hữu bộ từ vựng
        Task UpdateUserVocabularySetAsync(UserVocabularySet userVocabularySet);
    }
}