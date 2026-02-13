
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeSetRewardPetService : ISetRewardPetService
    {
        public Task<Pet?> GetRandomPetBySetIdAsync(
            int vocabularySetId,
            int milestone,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Pet?>(null);
        }
    }
}
