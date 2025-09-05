using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserOwnedPetService
    {
        // Gán pet cho người dùng
        Task<(Pet? grantedPet, bool alreadyOwned, int bonusXp)> TryGrantPetByMilestoneAsync(int userId, int vocabularySetId);
    }
}