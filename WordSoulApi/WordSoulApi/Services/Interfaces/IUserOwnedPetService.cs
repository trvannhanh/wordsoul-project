using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserOwnedPetService
    {
        Task<(bool alreadyOwned, int bonusXp)> GrantPetAsync(int userId, int petId);
    }
}