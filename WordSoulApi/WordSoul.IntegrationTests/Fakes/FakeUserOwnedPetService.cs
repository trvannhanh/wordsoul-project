
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeUserOwnedPetService : IUserOwnedPetService
    {
        public Task<UserOwnedPetDto?> AssignPetToUserAsync(
            AssignPetDto assignDto,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<UserOwnedPetDto?>(null);
        }

        public Task<(bool alreadyOwned, bool isSuccess, int bonusXp)> GrantPetAsync(
            int userId,
            int petId,
            double catchRate,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult((false, false, 0));
        }

        public Task<UpgradePetDto?> UpgradePetForUserAsync(
            int userId,
            int petId,
            int experience = 10,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<UpgradePetDto?>(null);
        }

        public Task<UserPetDetailDto?> ActivePetAsync(
            int userId,
            int petId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<UserPetDetailDto?>(null);
        }

        public Task<bool> RemovePetFromUserAsync(
            int userId,
            int petId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }
}
