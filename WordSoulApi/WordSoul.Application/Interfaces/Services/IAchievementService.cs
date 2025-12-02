using WordSoul.Application.DTOs.Achievement;
using WordSoul.Domain.Enums;
using WordSoul.Domain.Exceptions;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service quản lý Thành tựu (Achievement), bao gồm tạo mới,
    /// truy vấn theo bộ lọc và lấy chi tiết theo ID.
    /// </summary>
    public interface IAchievementService
    {
        /// <summary>
        /// Tạo mới một thành tựu.
        /// </summary>
        /// <param name="createAchievementDto">
        /// DTO chứa thông tin cần thiết để tạo thành tựu mới.
        /// </param>
        /// <param name="ct">
        /// Token hủy thao tác bất đồng bộ.
        /// </param>
        /// <returns>
        /// <see cref="AchievementDto"/> đại diện cho thành tựu vừa được tạo.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Ném ra khi trường Tên (Name) bị rỗng hoặc không hợp lệ.
        /// </exception>
        /// <exception cref="Exception">
        /// Ném ra khi xảy ra lỗi không mong muốn trong quá trình tạo.
        /// </exception>
        Task<AchievementDto> CreateAchievementAsync(
            CreateAchievementDto createAchievementDto,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy danh sách thành tựu với phân trang, có thể lọc theo loại điều kiện (ConditionType).
        /// </summary>
        /// <param name="conditionType">
        /// Loại điều kiện cần lọc. Nếu null sẽ lấy toàn bộ.
        /// </param>
        /// <param name="pageNumber">
        /// Số trang muốn lấy (phải &gt; 0).
        /// </param>
        /// <param name="pageSize">
        /// Số lượng phần tử trên mỗi trang (phải &gt; 0).
        /// </param>
        /// <param name="ct">
        /// Token hủy thao tác bất đồng bộ.
        /// </param>
        /// <returns>
        /// Danh sách <see cref="AchievementDto"/> theo bộ lọc và phân trang.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Ném ra khi <paramref name="pageNumber"/> hoặc <paramref name="pageSize"/> nhỏ hơn 1.
        /// </exception>
        Task<List<AchievementDto>> GetAchievementsAsync(
            ConditionType? conditionType,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy thông tin chi tiết một thành tựu theo ID.
        /// </summary>
        /// <param name="achievementId">
        /// ID của thành tựu cần truy vấn.
        /// </param>
        /// <param name="ct">
        /// Token hủy thao tác bất đồng bộ.
        /// </param>
        /// <returns>
        /// <see cref="AchievementDto"/> chứa thông tin chi tiết của thành tựu.
        /// </returns>
        /// <exception cref="NotFoundException">
        /// Ném ra khi không tìm thấy thành tựu với ID tương ứng.
        /// </exception>
        Task<AchievementDto> GetAchievementByIdAsync(
            int achievementId,
            CancellationToken ct = default);
    }
}
