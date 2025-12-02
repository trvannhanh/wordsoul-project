using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.User;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ xử lý tiến trình học từ vựng của người dùng.
    /// Bao gồm thống kê số từ cần ôn tập, thời gian ôn tập tiếp theo
    /// và phân bố số từ theo các mức độ thành thạo.
    /// </summary>
    public interface IUserVocabularyProgressService
    {
        /// <summary>
        /// Lấy thông tin tiến trình học từ vựng của người dùng, bao gồm:
        /// - Số từ cần ôn tập ngay hôm nay (NextReviewTime &lt;= hiện tại)
        /// - Thời gian ôn tập tiếp theo gần nhất
        /// - Thống kê số từ theo từng mức độ thành thạo (ProficiencyLevel)
        /// </summary>
        /// <param name="userId">ID của người dùng cần lấy thông tin.</param>
        /// <param name="cancellationToken">Token để hủy thao tác bất đồng bộ.</param>
        /// <returns>
        /// <see cref="UserProgressDto"/> chứa thông tin đầy đủ về tiến trình học từ vựng.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Khi người dùng không tồn tại trong hệ thống.</exception>
        Task<UserProgressDto> GetUserProgressAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
