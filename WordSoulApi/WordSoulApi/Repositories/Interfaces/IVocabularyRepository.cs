using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IVocabularyRepository
    {
        // Tạo từ vựng mới
        Task<Vocabulary> CreateVocabularyAsync(Vocabulary vocabulary);
        // Xóa từ vựng theo ID
        Task<bool> DeleteVocabularyAsync(int id);
        // Lấy tất cả các từ vựng
        Task<IEnumerable<Vocabulary>> GetAllVocabulariesAsync();
        // Lấy các từ vựng chưa học từ một bộ cụ thể cho người dùng
        Task<IEnumerable<Vocabulary>> GetUnlearnedVocabulariesFromSetAsync(int userId, int setId, int take = 5);
        Task<IEnumerable<Vocabulary>> GetUnreviewdVocabulariesFromSetAsync(int userId, int take = 5);
        Task<IEnumerable<Vocabulary>> GetVocabulariesBySessionIdAsync(int sessionId);

        // Lấy các từ vựng theo danh sách từ
        Task<IEnumerable<Vocabulary>> GetVocabulariesByWordsAsync(List<string> words);
        // Lấy từ vựng theo ID
        Task<Vocabulary?> GetVocabularyByIdAsync(int id);
        // Lấy danh sách ID từ vựng trong một phiên học cụ thể
        Task<IEnumerable<int>> GetVocabularyIdsBySessionIdAsync(int sessionId);
        // Cập nhật từ vựng
        Task<Vocabulary> UpdateVocabularyAsync(Vocabulary vocabulary);
    }
}