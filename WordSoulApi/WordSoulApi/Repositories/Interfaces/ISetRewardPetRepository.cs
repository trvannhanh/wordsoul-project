using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface ISetRewardPetRepository
    {
        // Lấy danh sách pet theo VocabularySetId
        Task<IEnumerable<SetRewardPet>> GetPetsByVocabularySetIdAsync(int vocabularySetId);
    }
}