using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IVocabularyRepository
    {
        //-------------------------------- CREATE -----------------------------------
        // Tạo mới Vocabulary
        Task<Vocabulary> CreateVocabularyAsync(Vocabulary vocabulary);
        //-------------------------------- READ -----------------------------------
        // Lấy tất cả Vocabulary với phân trang và lọc
        Task<IEnumerable<Vocabulary>> GetAllVocabulariesAsync(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber, int pageSize);
        // Lấy từ vựng theo danh sách từ
        Task<IEnumerable<Vocabulary>> GetVocabulariesByWordsAsync(List<string> words);
        // Lấy từ vựng theo id
        Task<Vocabulary?> GetVocabularyByIdAsync(int id);
        //-------------------------------- UPDATE -----------------------------------
        // Cập nhật từ vựng
        Task<Vocabulary> UpdateVocabularyAsync(Vocabulary vocabulary);
        //-------------------------------- DELETE -----------------------------------
        // Xóa từ vựng
        Task<bool> DeleteVocabularyAsync(int id);
    }
}