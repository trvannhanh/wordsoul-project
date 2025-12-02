using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.User;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ quản lý mối quan hệ giữa người dùng và bộ từ vựng.
    /// Bao gồm thêm bộ từ vựng vào thư viện học tập của người dùng
    /// và lấy thông tin tiến trình học của người dùng đối với một bộ cụ thể.
    /// </summary>
    public interface IUserVocabularySetService
    {
        /// <summary>
        /// Thêm một bộ từ vựng vào danh sách học tập của người dùng.
        /// Hệ thống sẽ kiểm tra:
        /// <list type="bullet">
        ///     <item>Người dùng có tồn tại hay không.</item>
        ///     <item>Bộ từ vựng có tồn tại hay không.</item>
        ///     <item>Quyền truy cập (public hoặc private nhưng phải là chủ sở hữu).</item>
        ///     <item>Người dùng đã sở hữu bộ từ này chưa.</item>
        /// </list>
        /// </summary>
        /// <param name="userId">ID người dùng cần thêm bộ từ vựng.</param>
        /// <param name="vocabSetId">ID bộ từ vựng được thêm.</param>
        /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
        /// <exception cref="KeyNotFoundException">Khi người dùng hoặc bộ từ vựng không tồn tại.</exception>
        /// <exception cref="InvalidOperationException">Khi không có quyền truy cập hoặc bộ từ đã tồn tại trong thư viện của người dùng.</exception>
        Task AddVocabularySetToUserAsync(
            int userId,
            int vocabSetId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin chi tiết về tiến trình học bộ từ vựng của người dùng,
        /// bao gồm:
        /// <list type="bullet">
        ///     <item>Số lần hoàn thành học bộ từ vựng.</item>
        ///     <item>Trạng thái đã hoàn thành hay chưa.</item>
        ///     <item>Ngày bắt đầu thêm bộ từ.</item>
        ///     <item>Bộ đó hiện có được kích hoạt hay không.</item>
        /// </list>
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="vocabSetId">ID bộ từ vựng cần lấy thông tin.</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns><see cref="UserVocabularySetDto"/> chứa thông tin tiến trình bộ từ vựng.</returns>
        /// <exception cref="KeyNotFoundException">Khi người dùng không sở hữu bộ từ vựng này.</exception>
        Task<UserVocabularySetDto> GetUserVocabularySetAsync(
            int userId,
            int vocabSetId,
            CancellationToken cancellationToken = default);
    }
}
