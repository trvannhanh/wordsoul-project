
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeDailyQuestService : IDailyQuestService
    {
        public Task GenerateDailyQuestsForUserAsync(int userId, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<List<UserDailyQuest>> GetUserDailyQuestsAsync(int userId, DateTime date, CancellationToken ct = default)
        {
            return Task.FromResult(new List<UserDailyQuest>());
        }

        public Task UpdateQuestProgressAsync(int userId, QuestType questType, int increment = 1, double? accuracy = null, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
