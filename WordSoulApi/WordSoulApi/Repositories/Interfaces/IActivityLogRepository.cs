using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IActivityLogRepository
    {
        Task CreateAsync(ActivityLog activityLog);
        Task<List<ActivityLog>> GetAllAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10);
        Task<List<ActivityLog>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<int> GetCountByUserIdAsync(int userId);
    }
}