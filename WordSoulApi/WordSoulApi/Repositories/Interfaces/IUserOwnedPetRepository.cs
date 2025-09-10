using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserOwnedPetRepository
    {
        Task<bool> CheckExistsUserOwnedPetAsync(int userId, int petId);

        // kiểm tra người dùng có sở hữu pet này chưa
        Task<bool> CheckPetOwnedByUserAsync(int userId, int petId);
        Task CreateUserOwnedPetAsync(UserOwnedPet userOwnedPet);
        Task DeleteUserOwnedPetAsync(UserOwnedPet userOwnedPet);
        Task<List<UserOwnedPet>> GetAllUserOwnedPetByUserIdAsync(int userId);
        Task<UserOwnedPet?> GetUserOwnedPetByUserAndPetIdAsync(int userId, int petId);
        Task UpdateUserOwnedPetAsync(UserOwnedPet userOwnedPet);
    }
}