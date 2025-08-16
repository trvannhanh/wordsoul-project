using WordSoulApi.Models.DTOs.Pet;

namespace WordSoulApi.Services.Interfaces
{
    public interface IPetService
    {
        Task<CreatePetDto> CreatePetAsync(CreatePetDto petDto);
        Task<bool> DeletePetAsync(int id);
        Task<IEnumerable<AdminPetDto>> GetAllPetsAsync();
        Task<AdminPetDto?> GetPetByIdAsync(int id);
        Task<AdminPetDto> UpdatePetAsync(int id, AdminPetDto petDto);
    }
}