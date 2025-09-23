
using CloudinaryDotNet.Actions;
using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IActivityLogService _activityLogService;
        public UserService(IUserRepository userRepository, IActivityLogService activityLogService)
        {
            _userRepository = userRepository;
            _activityLogService = activityLogService;
        }

        //---------------------------CREATE-----------------------

        //---------------------------GET--------------------------

        // Lấy tất cả người dùng
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string? name, string? email, UserRole? role, int pageNumber, int pageSize)
        {
            var users = await _userRepository.GetAllUsersAsync(name, email, role, null, null, pageNumber, pageSize);
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive
            });
        }

        // Lấy người dùng theo ID
        public async Task<UserDetailDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetUserWithRelationsAsync(userId);
            if (user == null) throw new Exception("User not found");

            var now = DateTime.UtcNow;

            // Streak (tính chuỗi ngày liên tục có học)
            var sessionDates = await _userRepository.GetLearningSessionDatesAsync(userId);
            int streakDays = CalculateStreak(sessionDates);
            return new UserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                TotalXP = user.XP,
                TotalAP = user.AP,
                Level = user.XP / 100, // Ví dụ: 100 XP = 1 level
                StreakDays = streakDays,
                PetCount = user.UserOwnedPets.Count,
                //PetActive = user.UserOwnedPets.FirstOrDefault(p => p.IsActive)?.Pet.Name,
                AvatarUrl = user.UserOwnedPets.FirstOrDefault(p => p.IsActive)?.Pet.ImageUrl,
            };
        }

        

        public async Task<List<LeaderBoardDto>> GetLeaderBoardAsync(bool? topXP, bool? topAP,  int pageNumber, int pageSize)
        {
            var users = await _userRepository.GetAllUsersAsync(null, null, UserRole.User, topXP, topAP, pageNumber, pageSize);
            return users.Select(u => new LeaderBoardDto
            {
                UserId = u.Id,
                UserName = u.Username,
                totalXP = u.XP,
                totalAP = u.AP
            }).ToList();
        }


        //---------------------------UPDATE-----------------------

        // Cập nhật thông tin người dùng
        public async Task<UserDto> UpdateUserAsync(int id, UserDto userDto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.Username = userDto.Username;
            user.Email = userDto.Email;
            //user.Role = userDto.Role.ToString();
            user.CreatedAt = userDto.CreatedAt;
            user.IsActive = userDto.IsActive;
            await _userRepository.UpdateUserAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }


        public async Task<bool> AssignRoleToUserAsync(int userId, string roleName)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            if (Enum.TryParse<UserRole>(roleName, true, out var role))
            {
                user.Role = role;
                await _userRepository.UpdateUserAsync(user);
                await _activityLogService.CreateActivityLogAsync(userId, "RoleAssigned", $"Assigned role: {roleName}");
                return true;
            }
            return false;
        }


        //---------------------------DELETE-----------------------
        // Xóa người dùng theo ID
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;
            return await _userRepository.DeleteUserAsync(id);
        }



        //---------------------------PRIVATE----------------------
        private int CalculateStreak(List<DateTime> dates)
        {
            if (!dates.Any()) return 0;

            int streak = 1;
            var today = DateTime.UtcNow.Date;

            if (dates[0] != today && dates[0] != today.AddDays(-1))
                return 0;

            for (int i = 0; i < dates.Count - 1; i++)
            {
                if ((dates[i] - dates[i + 1]).TotalDays == 1)
                    streak++;
                else
                    break;
            }
            return streak;
        }



    }
}
