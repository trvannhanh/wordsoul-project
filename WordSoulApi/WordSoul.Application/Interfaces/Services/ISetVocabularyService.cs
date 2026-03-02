using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.DTOs.VocabularySet;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ quản lý từ vựng thuộc các bộ từ vựng.
    /// Hỗ trợ thêm, lấy dữ liệu có phân trang + cache, và xóa liên kết từ bộ.
    /// </summary>
    public interface ISetVocabularyService
    {
        // ============================================================================
        // CREATE
        // ============================================================================

        /// <summary>
        /// Thêm một từ vựng mới vào bộ từ vựng.
        /// Kiểm tra xem bộ có tồn tại và từ có bị trùng trong bộ hay không.
        /// </summary>
        /// <param name="setId">ID của bộ từ vựng.</param>
        /// <param name="vocabularyDto">Thông tin từ vựng cần thêm.</param>
        /// <param name="imageUrl">URL hình ảnh đã upload (nếu có).</param>
        /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
        /// <returns>
        /// <see cref="AdminVocabularyDto"/> của từ vừa thêm,
        /// hoặc <c>null</c> nếu xảy ra lỗi bất thường.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Nếu bộ từ vựng không tồn tại.</exception>
        /// <exception cref="ArgumentException">Nếu từ đã tồn tại trong bộ.</exception>
        Task<AdminVocabularyDto?> AddVocabularyToSetAsync(
            int setId,
            CreateVocabularyInSetDto vocabularyDto,
            string? imageUrl,
            CancellationToken cancellationToken = default);


        // ============================================================================
        // READ
        // ============================================================================

        /// <summary>
        /// Lấy danh sách các từ vựng trong một bộ với phân trang.
        /// Kết quả được cache trong 15 phút theo từng trang.
        /// </summary>
        /// <param name="setId">ID của bộ từ vựng.</param>
        /// <param name="pageNumber">Số trang muốn lấy (mặc định = 1).</param>
        /// <param name="pageSize">Số lượng phần tử mỗi trang (mặc định = 10).</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>
        /// <see cref="PagedResult{T}"/> chứa danh sách <see cref="VocabularyDto"/>
        /// cùng thông tin phân trang.
        /// </returns>
        Task<PagedResult<VocabularyDto>> GetVocabulariesInSetAsync(
            int setId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin chi tiết đầy đủ của bộ từ vựng,
        /// bao gồm thông tin bộ và danh sách từ có phân trang (có cache).
        /// </summary>
        /// <param name="id">ID bộ từ vựng.</param>
        /// <param name="page">Trang hiện tại (mặc định = 1).</param>
        /// <param name="pageSize">Số từ mỗi trang.</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>
        /// <see cref="VocabularySetFullDetailDto"/> nếu tìm thấy,
        /// hoặc <c>null</c> nếu không tồn tại bộ từ vựng.
        /// </returns>
        Task<VocabularySetFullDetailDto?> GetVocabularySetFullDetailsAsync(
            int id,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);


        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Xóa liên kết giữa từ vựng và bộ từ vựng.
        /// Không xóa từ vựng thật trong hệ thống.
        /// </summary>
        /// <param name="setId">ID bộ từ vựng.</param>
        /// <param name="vocabId">ID từ vựng cần gỡ khỏi bộ.</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>
        /// <c>true</c> nếu xóa thành công,
        /// <c>false</c> nếu không tìm thấy liên kết.
        /// </returns>
        Task<bool> RemoveVocabularyFromSetAsync(
            int setId,
            int vocabId,
            CancellationToken cancellationToken = default);
    }
}
