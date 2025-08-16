using WordSoulApi.Models.DTOs.Vocabulary;

namespace WordSoulApi.Services.Interfaces
{
    public interface IVocabularyService
    {
        Task<AdminVocabularyDto> CreateVocabularyAsync(AdminVocabularyDto vocabularyDto);
        Task<bool> DeleteVocabularyAsync(int id);
        Task<IEnumerable<AdminVocabularyDto>> GetAllVocabulariesAsync();
        Task<IEnumerable<AdminVocabularyDto>> GetVocabulariesByWordsAsync(SearchVocabularyDto searchVocabularyDto);
        Task<AdminVocabularyDto?> GetVocabularyByIdAsync(int id);
        Task<AdminVocabularyDto> UpdateVocabularyAsync(int id, AdminVocabularyDto vocabularyDto);
    }
}