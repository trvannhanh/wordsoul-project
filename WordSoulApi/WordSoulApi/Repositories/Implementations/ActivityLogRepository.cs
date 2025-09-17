using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly WordSoulDbContext _context;
        private readonly ILogger<ActivityLogRepository> _logger;

        public ActivityLogRepository(WordSoulDbContext context, ILogger<ActivityLogRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // -------------------------------------CREATE-----------------------------------------

        // Tạo một bản ghi nhật ký hoạt động mới
        public async Task CreateActivityLogAsync(ActivityLog activityLog)
        {
            try
            {
                _logger.LogInformation("Creating activity log for user {UserId}, action: {Action}", activityLog.UserId, activityLog.Action);

                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created activity log for user {UserId}", activityLog.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity log for user {UserId}, action: {Action}", activityLog.UserId, activityLog.Action);
                throw;
            }
        }


        // -------------------------------------READ-------------------------------------------

        // Lấy nhật ký hoạt động của người dùng theo userId với phân trang
        public async Task<List<ActivityLog>> GetActivityLogsByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid pagination parameters: pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");
            }

            try
            {
                _logger.LogInformation("Retrieving activity logs for user {UserId}, page: {PageNumber}, size: {PageSize}", userId, pageNumber, pageSize);

                var logs = await _context.ActivityLogs
                    .Where(al => al.UserId == userId)
                    .OrderByDescending(al => al.Timestamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(al => al.User)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {LogCount} activity logs for user {UserId}", logs.Count, userId);
                return logs;
            }
            catch (Exception ex)    
            {
                _logger.LogError(ex, "Error retrieving activity logs for user {UserId}, page: {PageNumber}, size: {PageSize}", userId, pageNumber, pageSize);
                throw;
            }
        }

        // Lấy tất cả nhật ký hoạt động với tùy chọn lọc theo action và fromDate, có phân trang
        public async Task<List<ActivityLog>> GetAllActivityLogsAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid pagination parameters: pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");
            }

            try
            {
                _logger.LogInformation("Retrieving all activity logs, action: {Action}, fromDate: {FromDate}, page: {PageNumber}, size: {PageSize}",
                    action ?? "null", fromDate?.ToString("yyyy-MM-dd") ?? "null", pageNumber, pageSize);

                var query = _context.ActivityLogs
                    .Include(al => al.User)
                    .OrderByDescending(al => al.Timestamp)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(action))
                    query = query.Where(al => al.Action == action);

                if (fromDate.HasValue)
                    query = query.Where(al => al.Timestamp >= fromDate.Value);

                var logs = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {LogCount} activity logs", logs.Count);
                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activity logs, action: {Action}, fromDate: {FromDate}, page: {PageNumber}, size: {PageSize}",
                    action ?? "null", fromDate?.ToString("yyyy-MM-dd") ?? "null", pageNumber, pageSize);
                throw;
            }
        }

        // Lấy tổng số bản ghi nhật ký hoạt động của một người dùng cụ thể
        public async Task<int> GetActivityLogsCountByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Retrieving activity log count for user {UserId}", userId);

                var count = await _context.ActivityLogs.CountAsync(al => al.UserId == userId);

                _logger.LogInformation("Successfully retrieved activity log count {LogCount} for user {UserId}", count, userId);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity log count for user {UserId}", userId);
                throw;
            }
        }
    }
}
