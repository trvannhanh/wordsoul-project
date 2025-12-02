using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly WordSoulDbContext _context;

        public ActivityLogRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // -------------------------------------CREATE-----------------------------------------
        // Tạo một bản ghi nhật ký hoạt động mới
        public Task CreateActivityLogAsync(ActivityLog activityLog, CancellationToken cancellationToken = default)
        {
            _context.ActivityLogs.Add(activityLog);
            return Task.CompletedTask;
        }

        // -------------------------------------READ-------------------------------------------
        // Lấy nhật ký hoạt động của người dùng theo userId với phân trang
        public async Task<List<ActivityLog>> GetActivityLogsByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");
            }

            var logs = await _context.ActivityLogs
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(al => al.User)
                .ToListAsync(cancellationToken);

            return logs;
        }

        // Lấy tất cả nhật ký hoạt động với tùy chọn lọc theo action và fromDate, có phân trang
        public async Task<List<ActivityLog>> GetAllActivityLogsAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");
            }

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
                .ToListAsync(cancellationToken);

            return logs;
        }

        // Lấy tổng số bản ghi nhật ký hoạt động của một người dùng cụ thể
        public async Task<int> GetActivityLogsCountByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var count = await _context.ActivityLogs.CountAsync(al => al.UserId == userId, cancellationToken);
            return count;
        }
    }
}