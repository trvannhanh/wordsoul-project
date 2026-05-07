using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface quản lý từ vựng (Vocabulary).
    /// Hỗ trợ CRUD và các truy vấn có cache.
    /// </summary>
    public interface IVocabularyService
    {
        /// <summary>
        /// Tạo từ vựng mới (dành cho admin).
        /// </summary>
        /// <param name="dto">Thông tin từ vựng.</param>
        /// <param name="imageUrl">URL ảnh nếu có.</param>
        /// <param name="cancellationToken">Token hủy.</param>
        /// <returns>DTO của từ vựng sau khi tạo.</returns>
        Task<AdminVocabularyDto> CreateVocabularyAsync(
            CreateVocabularyDto dto,
            string? imageUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách từ vựng có lọc + phân trang (có cache 15 phút).
        /// </summary>
        Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync(
            string? word = null,
            string? meaning = null,
            PartOfSpeech? partOfSpeech = null,
            CEFRLevel? cefrLevel = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy chi tiết từ vựng theo ID (có cache 30 phút).
        /// </summary>
        Task<VocabularyDto?> GetVocabularyByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy nhiều từ vựng dựa trên danh sách từ (word list).
        /// </summary>
        Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(
            SearchVocabularyDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật từ vựng (admin only).
        /// </summary>
        Task<AdminVocabularyDto?> UpdateVocabularyAsync(
            int id,
            CreateVocabularyDto dto,
            string? imageUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa từ vựng (admin only).
        /// </summary>
        Task<bool> DeleteVocabularyAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
