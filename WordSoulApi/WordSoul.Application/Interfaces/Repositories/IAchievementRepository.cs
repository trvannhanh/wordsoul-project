

using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IAchievementRepository
    {
        // CREATE
        Task CreateAchievementAsync(Achievement achievement, CancellationToken cancellationToken = default);

        // READ
        Task<List<Achievement>> GetAchievementsAsync(
            ConditionType? conditionType,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<Achievement?> GetAchievementByIdAsync(
            int achievementId,
            CancellationToken cancellationToken = default);

        // UPDATE
        Task<Achievement?> UpdateAchievementAsync(
            Achievement achievement,
            CancellationToken cancellationToken = default);

        // DELETE
        Task<bool> DeleteAchievementAsync(
            int achievementId,
            CancellationToken cancellationToken = default);
    }
}