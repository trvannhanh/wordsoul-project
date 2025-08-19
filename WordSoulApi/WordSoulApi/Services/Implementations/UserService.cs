using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive
            });
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return null;
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        public async Task<UserDto> UpdateUserAsync(int id, UserDto userDto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.Role = userDto.Role;
            user.CreatedAt = userDto.CreatedAt;
            user.IsActive = userDto.IsActive;
            await _userRepository.UpdateUserAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;
            return await _userRepository.DeleteUserAsync(id);
        }

        public async Task AddVocabularySetToUserAsync(int userId, int vocabId)
        {
            var exists = _userRepository.CheckUserVocabualryExist(userId, vocabId);
            if (exists != null)
            {
                var relation = new UserVocabularySet
                {
                    UserId = userId,
                    VocabularySetId = vocabId
                };

                await _userRepository.AddVocabularySetToUserAsync(relation);
            }
        }
    }
}
