using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IActivityLogRepository
    {
        Task CreateActivityLogAsync(ActivityLog activityLog);
        Task<List<ActivityLog>> GetActivityLogsByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<List<ActivityLog>> GetAllActivityLogsAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10);
        Task<int> GetActivityLogsCountByUserIdAsync(int userId);
    }
}