using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserAchievementRepository
    {
        Task BulkCreateUserAchievementAsync(IEnumerable<UserAchievement> userAchievements);
        Task CreateUserAchievementAsync(UserAchievement userAchievement);
    }
}