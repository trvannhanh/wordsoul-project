using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserVocabularySetRepository
    {
        // Thêm sự sở hữu một bộ từ vựng cho người dùng
        Task AddVocabularySetToUserAsync(UserVocabularySet userVocabularySet);
        // Kiểm tra người dùng có sở hữu bộ từ vựng này chưa
        Task<bool> CheckUserVocabualryExist(int userId, int vocabId);
    }
}