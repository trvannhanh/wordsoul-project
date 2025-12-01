

using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserAchievementRepository
    {
        Task BulkCreateUserAchievementAsync(IEnumerable<UserAchievement> userAchievements);
        Task CreateUserAchievementAsync(UserAchievement userAchievement);
    }
}