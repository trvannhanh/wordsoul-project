
using WordSoul.Application.DTOs.DailyQuest;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeDailyQuestService : IDailyQuestService
    {
        public Task<ClaimQuestRewardResponseDto> ClaimRewardAsync(int userId, int userDailyQuestId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<DailyQuestDto> CreateQuestAsync(CreateDailyQuestDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task GenerateDailyQuestsForUserAsync(int userId, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<List<DailyQuestDto>> GetActiveQuestsAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserDailyQuest>> GetUserDailyQuestsAsync(int userId, DateTime date, CancellationToken ct = default)
        {
            return Task.FromResult(new List<UserDailyQuest>());
        }

        public Task ToggleQuestActiveAsync(int questId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateQuestProgressAsync(int userId, QuestType questType, int increment = 1, double? accuracy = null, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
