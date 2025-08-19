using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IVocabularySetRepository
    {
        Task<VocabularySet> CreateVocabularySetAsync(VocabularySet vocabularySet);
        Task<bool> DeleteVocabularySetAsync(int id);
        Task<IEnumerable<VocabularySet>> GetAllVocabularySetsAsync(int pageNumber = 1, int pageSize = 10);
        Task<VocabularySet?> GetVocabularySetByIdAsync(int id);
        Task<IEnumerable<VocabularySet>> SearchVocabularySetAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty, DateTime? createdAfter, int pageNumber, int pageSize);
        Task<VocabularySet?> UpdateVocabularySetAsync(VocabularySet vocabularySet);
    }
}