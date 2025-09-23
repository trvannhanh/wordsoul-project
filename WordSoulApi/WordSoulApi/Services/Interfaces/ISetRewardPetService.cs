using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface ISetRewardPetService
    {
        Task<Pet?> GetRandomPetBySetIdAsync(int vocabularySetId, int milestone);
    }
}