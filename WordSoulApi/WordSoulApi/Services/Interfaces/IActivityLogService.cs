using WordSoulApi.Models.DTOs;

namespace WordSoulApi.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task CreateActivityLogAsync(int userId, string action, string details);
        Task<List<ActivityLogDto>> GetAllActivityLogsAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10);
        Task<List<ActivityLogDto>> GetUserActivityLogsAsync(int userId, int pageNumber = 1, int pageSize = 10);
    }
}