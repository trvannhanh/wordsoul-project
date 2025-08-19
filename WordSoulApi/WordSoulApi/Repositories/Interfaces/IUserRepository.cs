using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task AddVocabularySetToUserAsync(UserVocabularySet userVocabularySet);
        Task<bool> CheckUserVocabualryExist(int userId, int vocabId);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User> UpdateUserAsync(User user);
    }
}