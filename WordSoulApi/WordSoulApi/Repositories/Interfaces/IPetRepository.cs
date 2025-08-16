using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Implementations
{
    public interface IPetRepository
    {
        Task<Pet> CreatePetAsync(Pet pet);
        Task<bool> DeletePetAsync(int id);
        Task<IEnumerable<Pet>> GetAllPetsAsync();
        Task<Pet?> GetPetByIdAsync(int id);
        Task<Pet> UpdatePetAsync(Pet pet);
    }
}