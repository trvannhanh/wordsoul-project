
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IDailyQuestRepository
    {
        Task<List<DailyQuest>> GetActiveQuestsAsync(
            CancellationToken cancellationToken = default);

        Task<DailyQuest?> GetByIdAsync(
            int questId,
            CancellationToken cancellationToken = default);

        Task CreateQuestAsync(DailyQuest quest, CancellationToken ct = default);
        Task UpdateQuestAsync(DailyQuest quest, CancellationToken ct = default);
    }
}
