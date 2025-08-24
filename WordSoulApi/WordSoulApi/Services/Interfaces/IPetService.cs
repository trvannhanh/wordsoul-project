using WordSoulApi.Models.DTOs.Pet;

namespace WordSoulApi.Services.Interfaces
{
    public interface IPetService
    {
        // Tạo một con pet mới
        Task<PetDto> CreatePetAsync(CreatePetDto petDto);
        // Xóa con pet theo ID
        Task<bool> DeletePetAsync(int id);
        // Lấy tất cả các con pet
        Task<IEnumerable<PetDto>> GetAllPetsAsync();
        // Lấy con pet theo ID
        Task<PetDto?> GetPetByIdAsync(int id);
        // Cập nhật con pet
        Task<AdminPetDto> UpdatePetAsync(int id, AdminPetDto petDto);
    }
}