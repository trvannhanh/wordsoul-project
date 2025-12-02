

using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IActivityLogRepository
    {
        // CREATE
        Task CreateActivityLogAsync(
            ActivityLog activityLog,
            CancellationToken cancellationToken = default);

        // READ - lấy theo UserId có phân trang
        Task<List<ActivityLog>> GetActivityLogsByUserIdAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        // READ - lấy tất cả có lọc action + fromDate + phân trang
        Task<List<ActivityLog>> GetAllActivityLogsAsync(
            string? action = null,
            DateTime? fromDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        // COUNT - đếm số log theo userId
        Task<int> GetActivityLogsCountByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}