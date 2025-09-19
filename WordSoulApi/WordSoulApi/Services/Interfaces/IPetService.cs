using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IPetService
    {

        // Tạo một con pet mới
        Task<PetDto> CreatePetAsync(CreatePetDto petDto, string? imageUrl);
        // Tạo nhiều con pet mới
        Task<List<PetDto>> CreatePetsBulkAsync(BulkCreatePetDto bulkDto);

        // Xóa con pet theo ID
        Task<bool> DeletePetAsync(int id);
        // Xóa nhiều con pet theo ID
        Task<bool> DeletePetsBulkAsync(List<int> petIds);

        // Lấy tất cả các con pet
        Task<IEnumerable<UserPetDto>> GetAllPetsAsync(int userId, string? name, PetRarity? rarity, PetType? type, bool? isOwned, int pageNumber, int pageSize);

        // Lấy con pet theo ID
        Task<PetDto?> GetPetByIdAsync(int id);
        // Lấy chi tiết con pet của người dùng theo ID
        Task<UserPetDetailDto?> GetPetDetailAsync(int userId, int petId);

        // Cập nhật con pet
        Task<AdminPetDto> UpdatePetAsync(int id, UpdatePetDto petDto, string? imageUrl);
        // Cập nhật nhiều con pet
        Task<List<PetDto>> UpdatePetsBulkAsync(List<UpdatePetDto> pets);
    }
}