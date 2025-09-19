using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserOwnedPetService
    {
        // Gán Pet cho người dùng 
        Task<UserOwnedPetDto?> AssignPetToUserAsync(AssignPetDto assignDto);

        //Gán pet cho user, trả về true nếu user đã sở hữu pet, false nếu mới được gán và số xp thưởng (nếu có)
        Task<(bool alreadyOwned, int bonusXp)> GrantPetAsync(int userId, int petId);
        // Gỡ pet khỏi người dùng
        Task<bool> RemovePetFromUserAsync(int userId, int petId);
        // nâng cấp pet cho người dùng
        Task<UpgradePetDto?> UpgradePetForUserAsync(int userId, int petId, int experience = 10);
    }
}