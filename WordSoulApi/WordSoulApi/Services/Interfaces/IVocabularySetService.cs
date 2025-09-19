using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IVocabularySetService
    {
        //Tạo mới bộ từ vựng
        Task<VocabularySetDto> CreateVocabularySetAsync(CreateVocabularySetDto createDto, string? imageUrl, int userId);
        //Xóa bộ từ vựng
        Task<bool> DeleteVocabularySetAsync(int id);
        //Lấy tất cả bộ từ vựng với tùy chọn lọc và phân trang
        Task<IEnumerable<VocabularySetDto>> GetAllVocabularySetsAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty, DateTime? createdAfter, bool? isOwned, int? userId, int pageNumber, int pageSize);
        //Lấy bộ từ vựng theo Id
        Task<VocabularySetDetailDto?> GetVocabularySetByIdAsync(int id);
        //Cập nhật bộ từ vựng
        Task<VocabularySetDto?> UpdateVocabularySetAsync(int id, UpdateVocabularySetDto updateDto);
    }
}