using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IVocabularySetRepository
    {
        Task<int> CountVocabulariesInSetAsync(int vocabularySetId);

        // Tạo mới bộ từ vựng
        Task<VocabularySet> CreateVocabularySetAsync(VocabularySet vocabularySet);
        // Xóa bộ từ vựng theo ID
        Task<bool> DeleteVocabularySetAsync(int id);

        // Lấy bộ từ vựng theo ID
        Task<VocabularySet?> GetVocabularySetByIdAsync(int id);
        Task<VocabularySet?> GetVocabularySetFullDetailsAsync(int id, int page, int pageSize);

        // Lấy tất cả bộ từ vựng với các tiêu chí khác nhau và phân trang
        Task<IEnumerable<VocabularySet>> GetAllVocabularySetsAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty, DateTime? createdAfter, int pageNumber, int pageSize);
        // Cập nhật bộ từ vựng
        Task<VocabularySet?> UpdateVocabularySetAsync(VocabularySet vocabularySet);
    }
}