using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IVocabularySetService
    {
        Task<VocabularySetDto> CreateVocabularySetAsync(CreateVocabularySetDto createDto);
        Task<bool> DeleteVocabularySetAsync(int id);
        Task<IEnumerable<VocabularySetDto>> GetAllVocabularySetsAsync(int pageNumber = 1, int pageSize = 10);
        Task<VocabularySetDto?> GetVocabularySetByIdAsync(int id);
        Task<IEnumerable<VocabularySetDto>> SearchVocabularySetAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty, DateTime? createdAfter, int pageNumber, int pageSize);
        Task<VocabularySetDto?> UpdateVocabularySetAsync(int id, UpdateVocabularySetDto updateDto);
    }
}