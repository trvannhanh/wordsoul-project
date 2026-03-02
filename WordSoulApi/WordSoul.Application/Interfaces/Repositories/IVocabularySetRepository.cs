using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IVocabularySetRepository
    {
        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Tạo một bộ từ vựng mới trong hệ thống.
        /// </summary>
        /// <returns>Đối tượng VocabularySet vừa được thêm.</returns>
        Task<VocabularySet> CreateVocabularySetAsync(
            VocabularySet vocabularySet,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        /// <summary>
        /// Lấy chi tiết một bộ từ vựng theo Id, bao gồm danh sách từ vựng bên trong.
        /// </summary>
        Task<VocabularySet?> GetVocabularySetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tìm kiếm và lấy danh sách bộ từ vựng với nhiều tiêu chí lọc nâng cao và phân trang.
        /// Hỗ trợ lọc theo tiêu đề, chủ đề, độ khó, ngày tạo, trạng thái sở hữu (isOwned), 
        /// và tự động hiển thị các set công khai hoặc đã mở khóa của người dùng.
        /// </summary>
        Task<List<VocabularySet>> GetAllVocabularySetsAsync(
            string? title,
            VocabularySetTheme? theme,
            VocabularyDifficultyLevel? difficulty,
            DateTime? createdAfter,
            bool? isOwned,
            int? userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        /// <summary>
        /// Cập nhật thông tin bộ từ vựng (tiêu đề, mô tả, độ khó, trạng thái...).
        /// </summary>
        /// <returns>Đối tượng VocabularySet đã cập nhật, hoặc null nếu không tồn tại.</returns>
        Task<VocabularySet?> UpdateVocabularySetAsync(
            VocabularySet vocabularySet,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE -----------------------------
        /// <summary>
        /// Xóa bộ từ vựng theo Id.
        /// </summary>
        /// <returns>true nếu xóa thành công, false nếu không tìm thấy bộ từ vựng.</returns>
        Task<bool> DeleteVocabularySetAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}