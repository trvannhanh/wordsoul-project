using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserVocabularySetRepository
    {
        // Thêm sự sở hữu một bộ từ vựng cho người dùng
        Task AddVocabularySetToUserAsync(UserVocabularySet userVocabularySet);
        // Kiểm tra người dùng có sở hữu bộ từ vựng này chưa
        Task<bool> CheckUserVocabularyExist(int userId, int vocabId);
        Task<List<UserVocabularySet>> GetAllUserVocabularySetsAsync(int userId);

        // Đếm số session đã hoàn thành cho một người dùng trong một bộ từ vựng
        //Task<int> GetCompletedLearningSessionAsync(int userId, int vocabularySetId);
        Task<UserVocabularySet?> GetUserVocabularySetAsync(int userId, int vocabularySetId);
        Task UpdateCompletedLearningSessionAsync(int userId, int vocabularySetId, int increment = 1);
        Task UpdateUserVocabularySetAsync(UserVocabularySet userVocabularySet);
    }
}