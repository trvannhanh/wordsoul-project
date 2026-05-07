using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IVocabularyRepository
    {
        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Tạo một từ vựng mới trong hệ thống.
        /// </summary>
        /// <returns>Đối tượng Vocabulary vừa được thêm.</returns>
        Task<Vocabulary> CreateVocabularyAsync(
            Vocabulary vocabulary,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        /// <summary>
        /// Lấy danh sách từ vựng với bộ lọc và phân trang.
        /// Hỗ trợ tìm kiếm theo từ, nghĩa, loại từ (PartOfSpeech) và mức độ CEFR.
        /// </summary>
        Task<List<Vocabulary>> GetAllVocabulariesAsync(
            string? word,
            string? meaning,
            PartOfSpeech? partOfSpeech,
            CEFRLevel? cEFRLevel,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin từ vựng theo Id.
        /// </summary>
        Task<Vocabulary?> GetVocabularyByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách từ vựng theo danh sách các từ (word).
        /// So sánh không phân biệt hoa thường.
        /// </summary>
        Task<List<Vocabulary>> GetVocabulariesByWordsAsync(
            List<string> words,
            int? userId = null,
            CancellationToken cancellationToken = default);

        Task<List<Vocabulary>> GetVocabulariesByIdsAsync(
            List<int> ids,
            CancellationToken ct = default);

        // ----------------------------- UPDATE -----------------------------
        /// <summary>
        /// Cập nhật thông tin từ vựng.
        /// </summary>
        /// <returns>Đối tượng Vocabulary sau khi cập nhật.</returns>
        Task<Vocabulary> UpdateVocabularyAsync(
            Vocabulary vocabulary,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE -----------------------------
        /// <summary>
        /// Xóa từ vựng theo Id.
        /// </summary>
        /// <returns>true nếu xóa thành công, false nếu không tìm thấy từ vựng.</returns>
        Task<bool> DeleteVocabularyAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}