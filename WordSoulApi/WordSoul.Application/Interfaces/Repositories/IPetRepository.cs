using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IPetRepository
    {
        //-------------------------------- CREATE -----------------------------------
        // Tạo pet mới
        Task<Pet> CreatePetAsync(Pet pet);

        //-------------------------------- READ -----------------------------------
        // Lấy tất cả pet
        //Task<IEnumerable<(Pet Pet, bool IsOwned)>> GetAllPetsAsync(int userId, PetFilter filter);

        // Lấy pet theo ID
        Task<Pet?> GetPetByIdAsync(int id);
        Task<List<Pet>> GetRandomPetsByRarityAsync(PetRarity rarity, int count);

        //-------------------------------- UPDATE -----------------------------------
        // Cập nhật pet
        Task<Pet> UpdatePetAsync(Pet pet);

        //------------------------------- DELETE -----------------------------------
        // Xóa pet theo ID
        Task<bool> DeletePetAsync(int id);

    }
}