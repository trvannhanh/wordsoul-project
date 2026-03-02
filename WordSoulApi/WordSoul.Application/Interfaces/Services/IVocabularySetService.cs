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
