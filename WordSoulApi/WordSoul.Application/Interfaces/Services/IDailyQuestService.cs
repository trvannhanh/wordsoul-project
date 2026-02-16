
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IDailyQuestService
    {
        Task GenerateDailyQuestsForUserAsync(
            int userId,
            CancellationToken ct = default);

        Task<List<UserDailyQuest>> GetUserDailyQuestsAsync(
            int userId,
            DateTime date,
            CancellationToken ct = default);

        Task UpdateQuestProgressAsync(
            int userId,
            QuestType questType,
            int increment = 1,
            double? accuracy = null,
            CancellationToken ct = default);
    }
}
