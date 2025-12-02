using WordSoul.Application.DTOs;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service xử lý nhật ký hoạt động của người dùng.
    /// </summary>
    public interface IActivityLogService
    {
        /// <summary>
        /// Tạo một nhật ký hoạt động mới.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="action">Hành động thực hiện.</param>
        /// <param name="details">Thông tin chi tiết.</param>
        /// <param name="ct">Token hủy.</param>
        Task CreateActivityLogAsync(
            int userId,
            string action,
            string details,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy nhật ký hoạt động của người dùng theo userId.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="pageNumber">Số trang.</param>
        /// <param name="pageSize">Số lượng mỗi trang.</param>
        /// <param name="ct">Token hủy.</param>
        Task<List<ActivityLogDto>> GetUserActivityLogsAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy tất cả nhật ký hoạt động, có thể lọc theo hành động hoặc ngày bắt đầu.
        /// </summary>
        /// <param name="action">Hành động lọc (tuỳ chọn).</param>
        /// <param name="fromDate">Ngày bắt đầu lọc (tuỳ chọn).</param>
        /// <param name="pageNumber">Số trang.</param>
        /// <param name="pageSize">Số lượng mỗi trang.</param>
        /// <param name="ct">Token hủy.</param>
        Task<List<ActivityLogDto>> GetAllActivityLogsAsync(
            string? action = null,
            DateTime? fromDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);
    }
}
