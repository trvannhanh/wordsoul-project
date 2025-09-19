using WordSoulApi.Models.DTOs;

namespace WordSoulApi.Services.Interfaces
{
    public interface IActivityLogService
    {
        //-------------------------------- CREATE -----------------------------------
        // Tạo một bản ghi nhật ký hoạt động mới
        Task CreateActivityLogAsync(int userId, string action, string details);
        //------------------------------- READ -------------------------------------
        // Lấy tất cả nhật ký hoạt động với tùy chọn lọc và phân trang
        Task<List<ActivityLogDto>> GetAllActivityLogsAsync(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10);
        // Lấy nhật ký hoạt động của người dùng theo userId với phân trang
        Task<List<ActivityLogDto>> GetUserActivityLogsAsync(int userId, int pageNumber = 1, int pageSize = 10);
    }
}