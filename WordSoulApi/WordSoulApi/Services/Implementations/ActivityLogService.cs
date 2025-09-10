using WordSoulApi.Models.DTOs;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IUserRepository _userRepository;  // Để lấy user info

        public ActivityLogService(IActivityLogRepository activityLogRepository, IUserRepository userRepository)
        {
            _activityLogRepository = activityLogRepository;
            _userRepository = userRepository;
        }

        public async Task CreateActivityAsync(int userId, string action, string details)
        {
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            await _activityLogRepository.CreateAsync(activityLog);
        }

        public async Task<List<ActivityLogDto>> GetUserActivitiesAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var logs = await _activityLogRepository.GetByUserIdAsync(userId, pageNumber, pageSize);
            var user = await _userRepository.GetUserByIdAsync(userId);  // Để lấy username

            return logs.Select(log => new ActivityLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Username = user?.Username ?? "Unknown",
                Action = log.Action,
                Details = log.Details,
                Timestamp = log.Timestamp
            }).ToList();
        }

        public async Task<List<ActivityLogDto>> GetAllActivitiesAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10)
        {
            var logs = await _activityLogRepository.GetAllAsync(action, fromDate, pageNumber, pageSize);

            return logs.Select(log => new ActivityLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Username = log.User?.Username ?? "Unknown",
                Action = log.Action,
                Details = log.Details,
                Timestamp = log.Timestamp
            }).ToList();
        }
    }
}
