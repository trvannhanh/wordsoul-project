using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface ISetRewardPetRepository
    {
        // Lấy danh sách pet theo VocabularySetId
        Task<IEnumerable<SetRewardPet>> GetPetsByVocabularySetIdAsync(int vocabularySetId);
    }
}