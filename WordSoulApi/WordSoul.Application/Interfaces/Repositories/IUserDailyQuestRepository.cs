
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserDailyQuestRepository
    {
        Task<UserDailyQuest?> GetUserDailyQuestAsync(
            int userId,
            int dailyQuestId,
            DateTime questDate,
            CancellationToken cancellationToken = default);

        Task<List<UserDailyQuest>> GetUserDailyQuestsByUserAndDateAsync(
            int userId,
            DateTime questDate,
            CancellationToken cancellationToken = default);

        Task<UserDailyQuest> CreateUserDailyQuestAsync(
            UserDailyQuest userDailyQuest,
            CancellationToken cancellationToken = default);

        Task UpdateUserDailyQuestAsync(
            UserDailyQuest userDailyQuest,
            CancellationToken cancellationToken = default);
    }
}
