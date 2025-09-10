using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IPetService
    {
        Task<UserOwnedPetDto?> AssignPetToUserAsync(AssignPetDto assignDto);

        // Tạo một con pet mới
        Task<PetDto> CreatePetAsync(CreatePetDto petDto, string? imageUrl);
        Task<List<PetDto>> CreatePetsBulkAsync(BulkCreatePetDto bulkDto);

        // Xóa con pet theo ID
        Task<bool> DeletePetAsync(int id);
        Task<bool> DeletePetsBulkAsync(List<int> petIds);
        Task<UserOwnedPetDto?> EvolvePetForUserAsync(EvolvePetDto evolveDto);

        // Lấy tất cả các con pet
        Task<IEnumerable<UserPetDto>> GetAllPetsAsync(int userId, string? name, PetRarity? rarity, PetType? type, bool? isOwned, int pageNumber, int pageSize);

        // Lấy con pet theo ID
        Task<PetDto?> GetPetByIdAsync(int id);
        Task<bool> RemovePetFromUserAsync(int userId, int petId);

        // Cập nhật con pet
        Task<AdminPetDto> UpdatePetAsync(int id, UpdatePetDto petDto, string? imageUrl);
        Task<List<PetDto>> UpdatePetsBulkAsync(List<UpdatePetDto> pets);
    }
}