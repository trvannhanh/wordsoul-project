using Microsoft.Extensions.Caching.Memory;
using WordSoulApi.Models.DTOs;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(IActivityLogRepository activityLogRepository, IMemoryCache cache, ILogger<ActivityLogService> logger)
        {
            _activityLogRepository = activityLogRepository;
            _cache = cache;
            _logger = logger;
        }

        // -------------------------------------CREATE-----------------------------------------

        // Tạo một bản ghi nhật ký hoạt động mới
        public async Task CreateActivityLogAsync(int userId, string action, string details)
        {
            if (userId <= 0)
                throw new ArgumentException("userId must be greater than 0.");
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("action cannot be null or empty.");

            try
            {
                _logger.LogInformation("Creating activity log for user {UserId}, action: {Action}", userId, action);

                var activityLog = new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };

                await _activityLogRepository.CreateActivityLogAsync(activityLog);
                _logger.LogInformation("Successfully created activity log for user {UserId}", userId);

                _cache.Remove($"ActivityLogs_User_{userId}_1_10"); // Xóa cache trang đầu tiên
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity log for user {UserId}, action: {Action}", userId, action);
                throw new Exception($"Error creating activity log for user {userId}: {ex.Message}", ex);
            }
        }

        // -------------------------------------READ-------------------------------------------

        // Lấy nhật ký hoạt động của người dùng theo userId với phân trang
        public async Task<List<ActivityLogDto>> GetUserActivityLogsAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");

            var cacheKey = $"ActivityLogs_User_{userId}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out List<ActivityLogDto> cachedLogs))
            {
                _logger.LogInformation("Retrieved activity logs for user {UserId} from cache, page: {PageNumber}, size: {PageSize}", userId, pageNumber, pageSize);
                return cachedLogs;
            }

            try
            {
                _logger.LogInformation("Retrieving activity logs for user {UserId}, page: {PageNumber}, size: {PageSize}", userId, pageNumber, pageSize);

                var logs = await _activityLogRepository.GetActivityLogsByUserIdAsync(userId, pageNumber, pageSize);
                var result = logs.Select(MapToDto).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("Successfully retrieved {LogCount} activity logs for user {UserId}", result.Count, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity logs for user {UserId}, page: {PageNumber}, size: {PageSize}", userId, pageNumber, pageSize);
                throw new Exception($"Error retrieving activity logs for user {userId}: {ex.Message}", ex);
            }
        }

        // Lấy tất cả nhật ký hoạt động với tùy chọn lọc theo action và fromDate, có phân trang
        public async Task<List<ActivityLogDto>> GetAllActivityLogsAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");
            if (pageSize > 100) // Giới hạn pageSize tối đa
            {
                _logger.LogWarning("pageSize {PageSize} exceeds maximum limit of 100, setting to 100", pageSize);
                pageSize = 100;
            }

            var cacheKey = $"ActivityLogs_All_{action ?? "null"}_{fromDate?.ToString("yyyyMMdd") ?? "null"}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out List<ActivityLogDto> cachedLogs))
            {
                _logger.LogInformation("Retrieved all activity logs from cache, action: {Action}, fromDate: {FromDate}, page: {PageNumber}, size: {PageSize}",
                    action ?? "null", fromDate?.ToString("yyyy-MM-dd") ?? "null", pageNumber, pageSize);
                return cachedLogs;
            }

            try
            {
                _logger.LogInformation("Retrieving all activity logs, action: {Action}, fromDate: {FromDate}, page: {PageNumber}, size: {PageSize}",
                    action ?? "null", fromDate?.ToString("yyyy-MM-dd") ?? "null", pageNumber, pageSize);

                var logs = await _activityLogRepository.GetAllActivityLogsAsync(action, fromDate, pageNumber, pageSize);
                var result = logs.Select(MapToDto).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("Successfully retrieved {LogCount} activity logs", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activity logs, action: {Action}, fromDate: {FromDate}, page: {PageNumber}, size: {PageSize}",
                    action ?? "null", fromDate?.ToString("yyyy-MM-dd") ?? "null", pageNumber, pageSize);
                throw new Exception($"Error retrieving all activity logs: {ex.Message}", ex);
            }
        }

        // Helper method to map ActivityLog entity to ActivityLogDto
        private ActivityLogDto MapToDto(ActivityLog log)
        {
            return new ActivityLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Username = log.User?.Username ?? "Unknown",
                Action = log.Action,
                Details = log.Details,
                Timestamp = log.Timestamp
            };
        }
    }
}
