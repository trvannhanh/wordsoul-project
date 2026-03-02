using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface ISetRewardPetRepository
    {
        /// <summary>
        /// Lấy danh sách pet thưởng theo VocabularySetId.
        /// Bao gồm thông tin Pet (Eager Loaded).
        /// </summary>
        Task<IEnumerable<SetRewardPet>> GetPetsByVocabularySetIdAsync(
            int vocabularySetId,
            CancellationToken cancellationToken = default
        );
    }
}