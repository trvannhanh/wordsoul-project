using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IPetService
    {
        // Tạo một con pet mới
        Task<PetDto> CreatePetAsync(CreatePetDto petDto, string? imageUrl);
        // Xóa con pet theo ID
        Task<bool> DeletePetAsync(int id);
        // Lấy tất cả các con pet
        Task<IEnumerable<UserPetDto>> GetAllPetsAsync(int userId, string? name, PetRarity? rarity, PetType? type, bool? isOwned, int pageNumber, int pageSize);

        // Lấy con pet theo ID
        Task<PetDto?> GetPetByIdAsync(int id);
        // Cập nhật con pet
        //Task<AdminPetDto> UpdatePetAsync(int id, AdminPetDto petDto);
    }
}