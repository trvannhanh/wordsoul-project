using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserOwnedPetRepository
    {
        //-------------------------------- CREATE -----------------------------------
        Task CreateUserOwnedPetAsync(UserOwnedPet userOwnedPet);
        //-------------------------------- READ -----------------------------------
        Task<Pet?> GetRandomPetBySetIdAsync(int vocabularySetId);
        Task<UserOwnedPet?> GetUserOwnedPetByUserAndPetIdAsync(int userId, int petId);
        //------------------------------- UPDATE -----------------------------------
        Task UpdateUserOwnedPetAsync(UserOwnedPet userOwnedPet);

        //-------------------------------- DELETE -----------------------------------
        // Xóa UserOwnedPet
        Task DeleteUserOwnedPetAsync(UserOwnedPet userOwnedPet);

        //------------------------------- OTHER ------------------------------------

        // kiểm tra người dùng có sở hữu pet này chưa
        Task<bool> CheckPetOwnedByUserAsync(int userId, int petId);
        

    }
}