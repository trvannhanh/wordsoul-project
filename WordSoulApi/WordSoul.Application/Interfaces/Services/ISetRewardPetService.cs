using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Services
{
    public interface ISetRewardPetService
    {
        Task<Pet?> GetRandomPetBySetIdAsync(int vocabularySetId, int milestone);
    }
}