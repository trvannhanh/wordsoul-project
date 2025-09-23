using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IVocabularySetRepository
    {
        //-------------------------------- CREATE -----------------------------------
        // Tạo mới VocabularySet
        Task<VocabularySet> CreateVocabularySetAsync(VocabularySet vocabularySet);

        //------------------------------- READ -----------------------------------
        // Lấy tất cả VocabularySet với tùy chọn lọc và phân trang
        Task<IEnumerable<VocabularySet>> GetAllVocabularySetsAsync(string? title, VocabularySetTheme? theme, 
                                                                    VocabularyDifficultyLevel? difficulty, 
                                                                    DateTime? createdAfter, bool? isOwned, 
                                                                    int? userId, int pageNumber, int pageSize);
        // Lấy VocabularySet theo ID
        Task<VocabularySet?> GetVocabularySetByIdAsync(int id);
        //------------------------------- UPDATE -----------------------------------
        // Cập nhật VocabularySet hiện có
        Task<VocabularySet?> UpdateVocabularySetAsync(VocabularySet vocabularySet);
        //------------------------------- DELETE -----------------------------------
        // Xóa VocabularySet theo ID
        Task<bool> DeleteVocabularySetAsync(int id);
        Task<List<VocabularySet>> GetAllVocabularySetsAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty, DateTime? createdAfter, bool? isOwned, int? userId, int pageNumber, int pageSize);
    }
}