using WordSoulApi.Models.DTOs;

namespace WordSoulApi.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task CreateActivityAsync(int userId, string action, string details);
        Task<List<ActivityLogDto>> GetAllActivitiesAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10);
        Task<List<ActivityLogDto>> GetUserActivitiesAsync(int userId, int pageNumber = 1, int pageSize = 10);
    }
}