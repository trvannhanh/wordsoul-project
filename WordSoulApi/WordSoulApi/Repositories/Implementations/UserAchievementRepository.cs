using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class UserAchievementRepository : IUserAchievementRepository
    {
        private readonly WordSoulDbContext _context;

        public UserAchievementRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserAchievementAsync(UserAchievement userAchievement)
        {
            await _context.UserAchievements.AddAsync(userAchievement);
            await _context.SaveChangesAsync();
        }

        public async Task BulkCreateUserAchievementAsync(IEnumerable<UserAchievement> userAchievements)
        {
            await _context.UserAchievements.AddRangeAsync(userAchievements);
            await _context.SaveChangesAsync();
        }
    }
}
