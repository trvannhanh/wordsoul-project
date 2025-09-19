using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IActivityLogRepository
    {
        //-------------------------------------CREATE-----------------------------------------

        // Tạo một bản ghi nhật ký hoạt động mới
        Task CreateActivityLogAsync(ActivityLog activityLog);

        //-------------------------------------READ-------------------------------------------

        // Lấy nhật ký hoạt động của người dùng theo userId với phân trang
        Task<List<ActivityLog>> GetActivityLogsByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10);
        // Lấy tất cả nhật ký hoạt động với tùy chọn lọc và phân trang
        Task<List<ActivityLog>> GetAllActivityLogsAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10);

        // Lấy tổng số lượng nhật ký hoạt động của người dùng theo userId
        Task<int> GetActivityLogsCountByUserIdAsync(int userId);
    }
}