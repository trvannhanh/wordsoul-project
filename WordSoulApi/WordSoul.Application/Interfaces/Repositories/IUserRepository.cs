using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        // ----------------------------- READ -----------------------------
        /// <summary>
        /// Lấy danh sách người dùng với các bộ lọc và phân trang.
        /// Hỗ trợ tìm kiếm theo tên, email, role và sắp xếp theo top XP hoặc top AP.
        /// </summary>
        Task<IEnumerable<User>> GetAllUsersAsync(
            string? name,
            string? email,
            UserRole? role,
            bool? topXP,
            bool? topAP,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin người dùng theo Id (chỉ dữ liệu cơ bản, AsNoTracking).
        /// </summary>
        Task<User?> GetUserByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin người dùng kèm đầy đủ các quan hệ (pets, progress...).
        /// Sử dụng SplitQuery để tối ưu hiệu suất khi dữ liệu lớn.
        /// </summary>
        Task<User?> GetUserWithRelationsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách các ngày người dùng có phiên học tập (dùng cho lịch học tập, heatmap...).
        /// </summary>
        Task<List<DateTime>> GetLearningSessionDatesAsync(
            int userId,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        /// <summary>
        /// Cập nhật toàn bộ thông tin người dùng.
        /// </summary>
        Task<User> UpdateUserAsync(
            User user,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cộng thêm XP và AP cho người dùng. Trả về tổng XP và AP mới sau khi cập nhật.
        /// </summary>
        Task<(int XP, int AP)> UpdateUserXPAndAPAsync(
            int userId,
            int xp,
            int ap,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE -----------------------------
        /// <summary>
        /// Xóa người dùng theo Id.
        /// </summary>
        /// <returns>true nếu xóa thành công, false nếu không tìm thấy người dùng.</returns>
        Task<bool> DeleteUserAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}