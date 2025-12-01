using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
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
