using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IVocabularyRepository
    {
        Task<Vocabulary> CreateVocabularyAsync(Vocabulary vocabulary);
        Task<bool> DeleteVocabularyAsync(int id);
        Task<IEnumerable<Vocabulary>> GetAllVocabulariesAsync();
        Task<IEnumerable<Vocabulary>> GetVocabulariesByWordsAsync(List<string> words);
        Task<Vocabulary?> GetVocabularyByIdAsync(int id);
        Task<Vocabulary> UpdateVocabularyAsync(Vocabulary vocabulary);
    }
}