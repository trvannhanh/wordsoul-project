using WordSoulApi.Models.DTOs.Vocabulary;

namespace WordSoulApi.Services.Interfaces
{
    public interface IVocabularyService
    {
        Task<VocabularyDto> CreateVocabularyAsync(VocabularyDto vocabularyDto);
        Task<bool> DeleteVocabularyAsync(int id);
        Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync();
        Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(SearchVocabularyDto searchVocabularyDto);
        Task<VocabularyDto?> GetVocabularyByIdAsync(int id);
        Task<VocabularyDto> UpdateVocabularyAsync(int id, VocabularyDto vocabularyDto);
    }
}