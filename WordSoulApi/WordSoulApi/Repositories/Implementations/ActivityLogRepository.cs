using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly WordSoulDbContext _context;

        public ActivityLogRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(ActivityLog activityLog)
        {
            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityLog>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            return await _context.ActivityLogs
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(al => al.User)  // Để lấy username
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetAllAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.ActivityLogs
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .AsQueryable();

            if (!string.IsNullOrEmpty(action))
                query = query.Where(al => al.Action.Contains(action));

            if (fromDate.HasValue)
                query = query.Where(al => al.Timestamp >= fromDate.Value);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            return await _context.ActivityLogs.CountAsync(al => al.UserId == userId);
        }
    }
}
