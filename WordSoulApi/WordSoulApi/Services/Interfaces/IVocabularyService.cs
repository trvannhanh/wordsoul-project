using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IVocabularyService
    {
        //Tạo mới từ vựng
        Task<AdminVocabularyDto> CreateVocabularyAsync(CreateVocabularyDto vocabularyDto, string? imageUrl);
        //Xóa từ vựng theo ID
        Task<bool> DeleteVocabularyAsync(int id);
        //Lấy tất cả từ vựng với phân trang và lọc
        Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber, int pageSize);
        //Tìm kiếm từ vựng theo danh sách từ
        Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(SearchVocabularyDto searchVocabularyDto);
        //Lấy từ vựng theo ID
        Task<VocabularyDto?> GetVocabularyByIdAsync(int id);
        //Cập nhật từ vựng theo ID
        Task<AdminVocabularyDto> UpdateVocabularyAsync(int id, CreateVocabularyDto vocabularyDto, string? imageUrl);
    }
}