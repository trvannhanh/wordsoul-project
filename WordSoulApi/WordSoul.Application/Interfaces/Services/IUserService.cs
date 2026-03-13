using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.User;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ xử lý logic liên quan đến người dùng:
    /// quản lý hồ sơ, thống kê, leaderboard, cập nhật thông tin, phân quyền và xóa người dùng.
    /// </summary>
    public interface IUserService
    {
        // ============================================================================
        // READ
        // ============================================================================

        /// <summary>
        /// Lấy danh sách người dùng với các bộ lọc tùy chọn như tên, email, vai trò
        /// và phân trang. Kết quả trả về dạng UserDto rút gọn.
        /// </summary>
        /// <param name="name">Tên hoặc một phần tên người dùng.</param>
        /// <param name="email">Email hoặc một phần email.</param>
        /// <param name="role">Vai trò cần lọc (Admin, User...).</param>
        /// <param name="pageNumber">Trang hiện tại, mặc định 1.</param>
        /// <param name="pageSize">Số bản ghi mỗi trang, mặc định 20.</param>
        /// <param name="cancellationToken">Token để hủy thao tác bất đồng bộ.</param>
        /// <returns>Danh sách người dùng dạng UserDto.</returns>
        Task<IEnumerable<UserDto>> GetAllUsersAsync(
            string? name = null,
            string? email = null,
            UserRole? role = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin chi tiết của người dùng bao gồm:
        /// profile, XP, AP, cấp độ, streak, số lượng pet, avatar pet active, v.v.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Thông tin chi tiết người dùng dạng UserDetailDto.</returns>
        /// <exception cref="KeyNotFoundException">Nếu không tìm thấy người dùng.</exception>
        Task<UserDetailDto> GetUserByIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy bảng xếp hạng người dùng theo XP hoặc AP.
        /// Hỗ trợ phân trang, trả về danh sách LeaderBoardDto.
        /// </summary>
        /// <param name="topXP">true để sắp theo XP giảm dần; false nếu không.</param>
        /// <param name="topAP">true để sắp theo AP giảm dần; false nếu không.</param>
        /// <param name="pageNumber">Trang hiện tại, mặc định 1.</param>
        /// <param name="pageSize">Số phần tử mỗi trang, mặc định 50.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Danh sách người chơi trên bảng xếp hạng.</returns>
        Task<List<LeaderBoardDto>> GetLeaderBoardAsync(
            bool? topXP = null,
            bool? topAP = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        // ============================================================================
        // UPDATE
        // ============================================================================

        /// <summary>
        /// Cập nhật thông tin cơ bản của người dùng (dành cho admin).
        /// Bao gồm username, email, trạng thái hoạt động.
        /// </summary>
        /// <param name="id">ID người dùng.</param>
        /// <param name="dto">Thông tin cần cập nhật.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Bản ghi người dùng sau khi cập nhật.</returns>
        Task<UserDto> UpdateUserAsync(
            int id,
            UpdateUserDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tiêu thụ 1 Hint của người dùng.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>true nếu tiêu thụ thành công, false nếu hết Hint.</returns>
        Task<bool> ConsumeHintAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gán vai trò cho người dùng (Admin/User).
        /// Lưu log hoạt động khi phân quyền thành công.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="roleName">Tên vai trò (Admin/User).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>true nếu phân quyền thành công; false nếu thất bại.</returns>
        Task<bool> AssignRoleToUserAsync(
            int userId,
            string roleName,
            CancellationToken cancellationToken = default);

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Xóa người dùng theo ID. Có thể là xóa mềm hoặc xóa cứng tùy repository.
        /// </summary>
        /// <param name="id">ID người dùng cần xóa.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>true nếu xóa thành công; false nếu user không tồn tại.</returns>
        Task<bool> DeleteUserAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
