using WordSoulApi.Models.DTOs.Vocabulary;

namespace WordSoulApi.Services.Interfaces
{
    public interface IVocabularyService
    {
        // Tạo một từ vựng mới
        Task<VocabularyDto> CreateVocabularyAsync(VocabularyDto vocabularyDto);
        // Xóa từ vựng theo ID
        Task<bool> DeleteVocabularyAsync(int id);
        // Lấy tất cả các từ vựng
        Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync();
        // Tìm kiếm từ vựng theo các từ khóa
        Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(SearchVocabularyDto searchVocabularyDto);
        // Lấy từ vựng theo ID
        Task<VocabularyDto?> GetVocabularyByIdAsync(int id);
        // Cập nhật từ vựng
        Task<VocabularyDto> UpdateVocabularyAsync(int id, VocabularyDto vocabularyDto);
    }
}