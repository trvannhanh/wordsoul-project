using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork uow,
            IActivityLogService activityLogService,
            ILogger<UserService> logger)
        {
            _uow = uow;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        // ============================================================================
        // READ
        // ============================================================================

        /// <summary>
        /// Lấy danh sách người dùng với bộ lọc và phân trang.
        /// </summary>
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(
            string? name = null,
            string? email = null,
            UserRole? role = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching users - Name: {Name}, Email: {Email}, Role: {Role}, Page: {Page}", 
                name, email, role, pageNumber);

            var users = await _uow.User.GetAllUsersAsync(name, email, role, null, null, pageNumber, pageSize, cancellationToken);

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

        /// <summary>
        /// Lấy thông tin chi tiết người dùng kèm thống kê: level, streak, pet active, v.v.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Khi không tìm thấy user.</exception>
        public async Task<UserDetailDto> GetUserByIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching detailed profile for user ID {UserId}", userId);

            var user = await _uow.User.GetUserWithRelationsAsync(userId, cancellationToken)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            var sessionDates = await _uow.User.GetLearningSessionDatesAsync(userId, cancellationToken);
            int streakDays = CalculateStreak(sessionDates);

            var activePet = user.UserOwnedPets?.FirstOrDefault(p => p.IsActive);

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
                Level = user.XP / 100, // 100 XP = 1 level
                StreakDays = streakDays,
                PetCount = user.UserOwnedPets?.Count ?? 0,
                AvatarUrl = activePet?.Pet.ImageUrl,
                PetActiveId = activePet?.PetId,
                //PetActiveName = activePet?.Pet.Name
            };
        }

        /// <summary>
        /// Lấy bảng xếp hạng người chơi theo XP hoặc AP.
        /// </summary>
        public async Task<List<LeaderBoardDto>> GetLeaderBoardAsync(
            bool? topXP = null,
            bool? topAP = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching leaderboard - TopXP: {TopXP}, TopAP: {TopAP}", topXP, topAP);

            var users = await _uow.User.GetAllUsersAsync(
                name: null,
                email: null,
                role: UserRole.User,
                topXP,
                topAP,
                pageNumber: pageNumber,
                pageSize: pageSize,
                cancellationToken: cancellationToken);

            return users.Select(u => new LeaderBoardDto
            {
                UserId = u.Id,
                UserName = u.Username,
                TotalXP = u.XP,
                TotalAP = u.AP,
                //Level = u.XP / 100,
                //AvatarUrl = u.UserOwnedPets?.FirstOrDefault(p => p.IsActive)?.Pet.ImageUrl
            }).ToList();
        }

        // ============================================================================
        // UPDATE
        // ============================================================================

        /// <summary>
        /// Cập nhật thông tin cơ bản của người dùng (admin only).
        /// </summary>
        public async Task<UserDto> UpdateUserAsync(
            int id,
            UpdateUserDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _uow.User.GetUserByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"User with ID {id} not found.");

            user.Username = dto.Username ?? user.Username;
            user.Email = user.Email;
            user.IsActive = user.IsActive;

            await _uow.User.UpdateUserAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} updated by admin", id);

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

        /// <summary>
        /// Gán vai trò (role) cho người dùng.
        /// </summary>
        /// <returns>true nếu gán thành công.</returns>
        public async Task<bool> AssignRoleToUserAsync(
            int userId,
            string roleName,
            CancellationToken cancellationToken = default)
        {
            var user = await _uow.User.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Attempt to assign role to non-existent user ID {UserId}", userId);
                return false;
            }

            if (!Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var role))
            {
                _logger.LogWarning("Invalid role name: {RoleName}", roleName);
                return false;
            }

            var oldRole = user.Role;
            user.Role = role;

            await _uow.User.UpdateUserAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            await _activityLogService.CreateActivityLogAsync(
                userId, "RoleAssigned", $"Role changed from {oldRole} → {role}", cancellationToken);

            _logger.LogInformation("Role assigned to user {UserId}: {Role}", userId, role);
            return true;
        }

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Xóa mềm hoặc cứng người dùng (tùy implementation trong repo).
        /// </summary>
        public async Task<bool> DeleteUserAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _uow.User.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
                return false;

            var result = await _uow.User.DeleteUserAsync(id, cancellationToken);
            if (result)
            {
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("User {UserId} has been deleted", id);
            }

            return result;
        }

        // ============================================================================
        // PRIVATE HELPERS
        // ============================================================================

        /// <summary>
        /// Tính chuỗi ngày học liên tiếp (streak) chính xác theo ngày UTC.
        /// </summary>
        private static int CalculateStreak(IReadOnlyList<DateTime> dates)
        {
            if (dates == null || dates.Count == 0) return 0;

            // Sắp xếp giảm dần và lấy ngày duy nhất
            var uniqueDates = dates
                .Select(d => d.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            var today  = DateTime.UtcNow.Date;

            // Nếu hôm nay hoặc hôm qua không có → streak = 0
            if (!uniqueDates.Contains(today) && !uniqueDates.Contains(today.AddDays(-1)))
                return 0;

            int streak = 0;
            var current = today;

            while (true)
            {
                if (uniqueDates.Contains(current))
                {
                    streak++;
                    current = current.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
    }
}