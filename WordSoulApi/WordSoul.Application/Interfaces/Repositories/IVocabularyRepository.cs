using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IVocabularyRepository
    {
        //-------------------------------- CREATE -----------------------------------
        // Tạo mới Vocabulary
        Task<Vocabulary> CreateVocabularyAsync(Vocabulary vocabulary);
        //-------------------------------- READ -----------------------------------
        // Lấy tất cả Vocabulary với phân trang và lọc
        Task<List<Vocabulary>> GetAllVocabulariesAsync(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber, int pageSize);
        // Lấy từ vựng theo danh sách từ
        Task<List<Vocabulary>> GetVocabulariesByWordsAsync(List<string> words);
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