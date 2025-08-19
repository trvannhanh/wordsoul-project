using WordSoulApi.Models.DTOs.Pet;

namespace WordSoulApi.Services.Interfaces
{
    public interface IPetService
    {
        Task<PetDto> CreatePetAsync(CreatePetDto petDto);
        Task<bool> DeletePetAsync(int id);
        Task<IEnumerable<PetDto>> GetAllPetsAsync();
        Task<PetDto?> GetPetByIdAsync(int id);
        Task<AdminPetDto> UpdatePetAsync(int id, AdminPetDto petDto);
    }
}