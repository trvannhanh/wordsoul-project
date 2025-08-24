using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IVocabularySetRepository
    {
        // Tạo mới bộ từ vựng
        Task<VocabularySet> CreateVocabularySetAsync(VocabularySet vocabularySet);
        // Xóa bộ từ vựng theo ID
        Task<bool> DeleteVocabularySetAsync(int id);
        // Lấy tất cả bộ từ vựng với phân trang
        Task<IEnumerable<VocabularySet>> GetAllVocabularySetsAsync(int pageNumber = 1, int pageSize = 10);
        // Lấy bộ từ vựng theo ID
        Task<VocabularySet?> GetVocabularySetByIdAsync(int id);
        // Tìm kiếm bộ từ vựng với các tiêu chí khác nhau và phân trang
        Task<IEnumerable<VocabularySet>> SearchVocabularySetAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty, DateTime? createdAfter, int pageNumber, int pageSize);
        // Cập nhật bộ từ vựng
        Task<VocabularySet?> UpdateVocabularySetAsync(VocabularySet vocabularySet);
    }
}