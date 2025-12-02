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

        //-------------------------------------CREATE-------------------------------------------

        // Tạo một UserAchievement
        public async Task CreateUserAchievementAsync(UserAchievement userAchievement, CancellationToken cancellationToken = default)
        {
            await _context.UserAchievements.AddAsync(userAchievement, cancellationToken);
        }

        // Tạo nhiều UserAchievement cùng lúc
        public async Task BulkCreateUserAchievementAsync(IEnumerable<UserAchievement> userAchievements, CancellationToken cancellationToken = default)
        {
            await _context.UserAchievements.AddRangeAsync(userAchievements, cancellationToken);
        }
    }
}