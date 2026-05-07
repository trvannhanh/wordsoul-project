using WordSoul.Application.DTOs.VocabularySet;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IVocabularySetService
    {
        // ========================================================================
        // CREATE
        // ========================================================================
        Task<VocabularySetDto> CreateVocabularySetAsync(
            CreateVocabularySetDto dto,
            string? imageUrl,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sinh dữ liệu nháp qua AI cho danh sách từ vựng.
        /// </summary>
        Task<List<VocabularyPreviewDto>> AiPreviewVocabularySetAsync(
            AiPreviewRequestDto dto,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạo bộ từ vựng với AI hỗ trợ: tự động sinh metadata, ảnh, audio cho từ chưa có trong DB.
        /// </summary>
        Task<AiCreateVocabularySetResultDto> AiCreateVocabularySetAsync(
            AiCreateVocabularySetDto dto,
            string? imageUrl,
            int userId,
            CancellationToken cancellationToken = default);

        // ========================================================================
        // READ
        // ========================================================================
        Task<VocabularySetDetailDto?> GetVocabularySetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<VocabularySetDto>> GetAllVocabularySetsAsync(
            string? title = null,
            VocabularySetTheme? theme = null,
            VocabularyDifficultyLevel? difficulty = null,
            DateTime? createdAfter = null,
            bool? isOwned = null,
            int? userId = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy toàn bộ bộ từ vựng thỏa mãn điều kiện, gom nhóm theo Theme để trả về 1 lần (tối ưu frontend N+1 API).
        /// </summary>
        Task<Dictionary<string, List<VocabularySetDto>>> GetGroupedVocabularySetsAsync(
            string? title = null,
            int? userId = null,
            int limitPerTheme = 6,
            CancellationToken cancellationToken = default);

        // ========================================================================
        // UPDATE
        // ========================================================================
        Task<VocabularySetDto?> UpdateVocabularySetAsync(
            int id,
            UpdateVocabularySetDto dto,
            CancellationToken cancellationToken = default);

        // ========================================================================
        // DELETE
        // ========================================================================
        Task<bool> DeleteVocabularySetAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
