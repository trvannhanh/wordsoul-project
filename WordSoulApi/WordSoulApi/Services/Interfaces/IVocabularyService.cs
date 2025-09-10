using WordSoulApi.Models.DTOs;
using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IVocabularyService
    {
        Task<AdminVocabularyDto?> AddVocabularyToSetAsync(int setId, CreateVocabularyInSetDto vocabularyDto, string? imageUrl);

        // Tạo một từ vựng mới
        Task<AdminVocabularyDto> CreateVocabularyAsync(CreateVocabularyDto vocabularyDto, string? imageUrl);
        // Xóa từ vựng theo ID
        Task<bool> DeleteVocabularyAsync(int id);
        // Lấy tất cả các từ vựng
        Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber, int pageSize);
        // Tìm kiếm từ vựng theo các từ khóa
        Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(SearchVocabularyDto searchVocabularyDto);
        Task<PagedResult<VocabularyDto>> GetVocabulariesInSetAsync(int setId, int pageNumber = 1, int pageSize = 10);

        // Lấy từ vựng theo ID
        Task<VocabularyDto?> GetVocabularyByIdAsync(int id);
        Task<bool> RemoveVocabularyFromSetAsync(int setId, int vocabId);

        // Cập nhật từ vựng
        Task<AdminVocabularyDto> UpdateVocabularyAsync(int id, CreateVocabularyDto vocabularyDto, string? imageUrl);
    }
}