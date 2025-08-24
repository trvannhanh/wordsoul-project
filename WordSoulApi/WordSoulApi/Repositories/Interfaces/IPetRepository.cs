using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IPetRepository
    {
        // Tạo pet mới
        Task<Pet> CreatePetAsync(Pet pet);
        // Xóa pet theo ID
        Task<bool> DeletePetAsync(int id);
        // Lấy tất cả pet
        Task<IEnumerable<Pet>> GetAllPetsAsync();
        // Lấy pet theo ID
        Task<Pet?> GetPetByIdAsync(int id);
        // Cập nhật pet
        Task<Pet> UpdatePetAsync(Pet pet);
    }
}