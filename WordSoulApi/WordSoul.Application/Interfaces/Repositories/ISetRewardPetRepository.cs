using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface ISetRewardPetRepository
    {
        Task<IEnumerable<SetRewardPet>> GetPetsByVocabularySetIdAsync(
            int vocabularySetId,
            CancellationToken cancellationToken = default
        );

        Task<SetRewardPet?> GetAsync(int vocabularySetId, int petId, CancellationToken ct = default);
        Task AddAsync(SetRewardPet entry, CancellationToken ct = default);
        void Update(SetRewardPet entry);
        void Remove(SetRewardPet entry);
    }
}