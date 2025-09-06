using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IVocabularySetService
    {
        // Tạo một bộ từ vựng mới
        Task<VocabularySetDto> CreateVocabularySetAsync(CreateVocabularySetDto createDto, string? imageUrl);
        // Xóa bộ từ vựng theo ID
        Task<bool> DeleteVocabularySetAsync(int id);

        // Lấy bộ từ vựng theo ID
        Task<VocabularySetDetailDto?> GetVocabularySetByIdAsync(int id);
        Task<VocabularySetFullDetailDto?> GetVocabularySetFullDetailsAsync(int id, int page, int pageSize);

        // Lấy tất cả bộ từ vựng theo tiêu đề, chủ đề, độ khó và ngày tạo với phân trang
        Task<IEnumerable<VocabularySetDto>> GetAllVocabularySetsAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty, DateTime? createdAfter, int pageNumber, int pageSize);
        // Cập nhật bộ từ vựng
        Task<VocabularySetDto?> UpdateVocabularySetAsync(int id, UpdateVocabularySetDto updateDto);
    }
}