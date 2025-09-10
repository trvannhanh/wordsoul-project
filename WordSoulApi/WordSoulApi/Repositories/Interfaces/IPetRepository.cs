using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IPetRepository
    {
        // Tạo pet mới
        Task<Pet> CreatePetAsync(Pet pet);
        // Xóa pet theo ID
        Task<bool> DeletePetAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<Pet>> GetAllAsync();

        // Lấy tất cả pet
        Task<IEnumerable<(Pet Pet, bool IsOwned)>> GetAllPetsAsync(int userId, string? name, PetRarity? rarity, PetType? type, bool? isOwned, int pageNumber, int pageSize);

        // Lấy pet theo ID
        Task<Pet?> GetPetByIdAsync(int id);
        // Cập nhật pet
        Task<Pet> UpdatePetAsync(Pet pet);
    }
}