using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
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