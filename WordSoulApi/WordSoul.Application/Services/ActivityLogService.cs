using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(IUnitOfWork uow, IMemoryCache cache, ILogger<ActivityLogService> logger)
        {
            _uow = uow;
            _cache = cache;
            _logger = logger;
        }

        // -------------------------------------CREATE-----------------------------------------

        public async Task CreateActivityLogAsync(
            int userId,
            string action,
            string details,
            CancellationToken ct = default)
        {
            if (userId <= 0)
                throw new ArgumentException("userId must be greater than 0.");
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("action cannot be null or empty.");

            try
            {
                _logger.LogInformation(
                    "Creating activity log for user {UserId}, action: {Action}",
                    userId, action);

                var activityLog = new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };

                await _uow.ActivityLog.CreateActivityLogAsync(activityLog, ct);
                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation("Successfully created activity log for user {UserId}", userId);

                _cache.Remove($"ActivityLogs_User_{userId}_1_10");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating activity log for user {UserId}, action: {Action}",
                    userId, action);

                throw new Exception(
                    $"Error creating activity log for user {userId}: {ex.Message}",
                    ex);
            }
        }

        // -------------------------------------READ-------------------------------------------

        public async Task<List<ActivityLogDto>> GetUserActivityLogsAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");

            var cacheKey = $"ActivityLogs_User_{userId}_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out List<ActivityLogDto> cached))
            {
                _logger.LogInformation(
                    "Retrieved activity logs for user {UserId} from cache (page={Page}, size={Size})",
                    userId, pageNumber, pageSize);

                return cached;
            }

            try
            {
                _logger.LogInformation(
                    "Retrieving activity logs for user {UserId}, page={Page}, size={Size}",
                    userId, pageNumber, pageSize);

                var logs = await _uow.ActivityLog
                    .GetActivityLogsByUserIdAsync(userId, pageNumber, pageSize, ct);

                var dtos = logs.Select(MapToDto).ToList();

                _cache.Set(
                    cacheKey,
                    dtos,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving activity logs for user {UserId}, page={Page}, size={Size}",
                    userId, pageNumber, pageSize);

                throw new Exception(
                    $"Error retrieving activity logs for user {userId}: {ex.Message}",
                    ex);
            }
        }

        public async Task<List<ActivityLogDto>> GetAllActivityLogsAsync(
            string? action = null,
            DateTime? fromDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");
            if (pageSize > 100)
            {
                _logger.LogWarning("pageSize {PageSize} exceeds maximum of 100. Forcing to 100.", pageSize);
                pageSize = 100;
            }

            var cacheKey =
                $"ActivityLogs_All_{action ?? "null"}_{fromDate?.ToString("yyyyMMdd") ?? "null"}_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out List<ActivityLogDto> cached))
            {
                _logger.LogInformation(
                    "Retrieved all activity logs from cache, action={Action}, fromDate={FromDate}, page={Page}, size={Size}",
                    action ?? "null",
                    fromDate?.ToString("yyyy-MM-dd") ?? "null",
                    pageNumber,
                    pageSize);

                return cached;
            }

            try
            {
                _logger.LogInformation(
                    "Retrieving all activity logs, action={Action}, fromDate={FromDate}, page={Page}, size={Size}",
                    action ?? "null",
                    fromDate?.ToString("yyyy-MM-dd") ?? "null",
                    pageNumber,
                    pageSize);

                var logs = await _uow.ActivityLog
                    .GetAllActivityLogsAsync(action, fromDate, pageNumber, pageSize, ct);

                var dtos = logs.Select(MapToDto).ToList();

                _cache.Set(
                    cacheKey,
                    dtos,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving all activity logs, action={Action}, fromDate={FromDate}, page={Page}, size={Size}",
                    action ?? "null",
                    fromDate?.ToString("yyyy-MM-dd") ?? "null",
                    pageNumber,
                    pageSize);

                throw new Exception($"Error retrieving all activity logs: {ex.Message}", ex);
            }
        }

        // --------------------------HELPER-------------------------------------

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
