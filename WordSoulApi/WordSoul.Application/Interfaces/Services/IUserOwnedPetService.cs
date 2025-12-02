using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Pet;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ quản lý Pet mà người dùng sở hữu:
    /// gán pet, bắt pet, nâng cấp pet, kích hoạt pet và gỡ pet khỏi người dùng.
    /// </summary>
    public interface IUserOwnedPetService
    {
        // ============================================================================
        // CREATE / GRANT
        // ============================================================================

        /// <summary>
        /// Gán trực tiếp một pet cho người dùng (thường dùng cho admin hoặc phần thưởng chắc chắn).
        /// </summary>
        /// <param name="assignDto">Thông tin gán pet (UserId, PetId, Level ban đầu…).</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>Thông tin UserOwnedPetDto sau khi gán, hoặc null nếu User/Pet không tồn tại.</returns>
        Task<UserOwnedPetDto?> AssignPetToUserAsync(
            AssignPetDto assignDto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cấp/bắt pet ngẫu nhiên theo tỷ lệ catchRate.
        /// Nếu người dùng đã sở hữu → thưởng XP.
        /// Nếu chưa sở hữu → thử bắt theo xác suất.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="petId">ID pet muốn cấp.</param>
        /// <param name="catchRate">Xác suất thành công (0–1).</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>
        /// Bộ 3 giá trị:
        /// <list type="bullet">
        ///   <item><description><c>alreadyOwned</c>: Người dùng đã sở hữu pet.</description></item>
        ///   <item><description><c>isSuccess</c>: Bắt thành công hay không.</description></item>
        ///   <item><description><c>bonusXp</c>: XP được tặng nếu đã sở hữu.</description></item>
        /// </list>
        /// </returns>
        Task<(bool alreadyOwned, bool isSuccess, int bonusXp)> GrantPetAsync(
            int userId,
            int petId,
            double catchRate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tăng kinh nghiệm cho pet của người dùng, tự động xử lý lên cấp và tiến hóa nếu đủ điều kiện.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="petId">ID pet đang nâng cấp.</param>
        /// <param name="experience">XP cộng thêm (mặc định 10).</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>Thông tin nâng cấp, bao gồm level mới, XP mới và trạng thái tiến hóa.</returns>
        Task<UpgradePetDto?> UpgradePetForUserAsync(
            int userId,
            int petId,
            int experience = 10,
            CancellationToken cancellationToken = default);

        // ============================================================================
        // UPDATE
        // ============================================================================

        /// <summary>
        /// Đặt pet làm pet đang active của người dùng.
        /// Chỉ có 1 pet active tại một thời điểm.
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="petId">ID pet cần kích hoạt.</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>Thông tin chi tiết pet sau khi kích hoạt.</returns>
        Task<UserPetDetailDto?> ActivePetAsync(
            int userId,
            int petId,
            CancellationToken cancellationToken = default);

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Gỡ bỏ pet khỏi người dùng (dùng cho admin hoặc hệ thống).
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="petId">ID pet cần gỡ.</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>true nếu xóa thành công; false nếu pet không thuộc sở hữu người dùng.</returns>
        Task<bool> RemovePetFromUserAsync(
            int userId,
            int petId,
            CancellationToken cancellationToken = default);
    }
}
