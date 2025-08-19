using WordSoulApi.Models.DTOs.User;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserService
    {
        Task AddVocabularySetToUserAsync(int userId, int vocabId);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> UpdateUserAsync(int id, UserDto userDto);
    }
}