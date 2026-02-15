
using WordSoul.Domain.Entities;

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
    }
}
