

using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IAchievementRepository
    {
        Task CreateAchievementAsync(Achievement achievement);
        Task DeleteAchievementAsync(int achievementId);
        Task<Achievement?> GetAchievementByIdAsync(int achievementId);
        Task<List<Achievement>> GetAchievementsAsync(ConditionType? conditionType, int pageNumber, int pageSize);
        Task<Achievement> UpdateAchievementAsync(Achievement achievement);
    }
}