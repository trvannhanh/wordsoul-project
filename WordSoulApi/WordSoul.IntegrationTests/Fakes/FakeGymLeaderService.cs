using WordSoul.Application.DTOs.Gym;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.IntegrationTests.Fakes
{
    /// <summary>
    /// No-op implementation of IGymLeaderService for integration tests.
    /// </summary>
    public class FakeGymLeaderService : IGymLeaderService
    {
        public Task<List<GymLeaderDto>> GetAllGymsForUserAsync(int userId, CancellationToken ct = default)
            => Task.FromResult(new List<GymLeaderDto>());

        public Task<GymLeaderDto?> GetGymDetailAsync(int userId, int gymId, CancellationToken ct = default)
            => Task.FromResult<GymLeaderDto?>(null);

        public Task CheckAndUnlockGymsAsync(int userId, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
