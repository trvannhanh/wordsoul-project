using Microsoft.EntityFrameworkCore;
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

        public async Task<UserAchievement?> GetUserAchievementAsync(int userId, int achievementId, CancellationToken ct = default)
        {
            return await _context.UserAchievements
                .Include(x => x.Achievement)
                .FirstOrDefaultAsync(
                    x => x.UserId == userId && x.AchievementId == achievementId,
                    ct);
        }

        public async Task<List<UserAchievement>> GetUserAchievementByUserAsync(int userId,
        CancellationToken ct = default)
        {
            return await _context.UserAchievements
                .Include(x => x.Achievement)
                .Where(x => x.UserId == userId)
                .ToListAsync(ct);
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


        public Task UpdateUserAchievementAsync( UserAchievement userAchievement, CancellationToken ct = default)
        {
            _context.UserAchievements.Update(userAchievement);
            return Task.CompletedTask;
        }
    }
}