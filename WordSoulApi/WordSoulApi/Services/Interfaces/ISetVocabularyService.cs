using WordSoulApi.Models.DTOs;
using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.DTOs.VocabularySet;

namespace WordSoulApi.Services.Interfaces
{
    public interface ISetVocabularyService
    {
        // Tạo từ vựng mới và thêm vào bộ từ vựng
        Task<AdminVocabularyDto?> AddVocabularyToSetAsync(int setId, CreateVocabularyInSetDto vocabularyDto, string? imageUrl);
        // Thêm từ vựng hiện có vào bộ từ vựng
        Task<PagedResult<VocabularyDto>> GetVocabulariesInSetAsync(int setId, int pageNumber = 1, int pageSize = 10);
        // Lấy chi tiết đầy đủ của bộ từ vựng bao gồm danh sách từ vựng với phân trang
        Task<VocabularySetFullDetailDto?> GetVocabularySetFullDetailsAsync(int id, int page, int pageSize);
        // Xóa từ vựng khỏi bộ từ vựng
        Task<bool> RemoveVocabularyFromSetAsync(int setId, int vocabId);
    }
}