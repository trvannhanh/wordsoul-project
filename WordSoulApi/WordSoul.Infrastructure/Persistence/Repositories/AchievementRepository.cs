using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class AchievementRepository : IAchievementRepository
    {
        private readonly WordSoulDbContext _context;

        public AchievementRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //-----------------------CREATE-------------------
        public async Task CreateAchievementAsync(Achievement achievement, CancellationToken cancellationToken = default)
        {
            await _context.Achievements.AddAsync(achievement, cancellationToken);
        }

        //-----------------------READ---------------------
        public async Task<List<Achievement>> GetAchievementsAsync(ConditionType? conditionType, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Achievements
                .AsNoTracking()
                .AsQueryable();

            if (conditionType.HasValue)
                query = query.Where(a => a.ConditionType == conditionType);

            return await query
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync(cancellationToken);
        }

        public async Task<Achievement?> GetAchievementByIdAsync(int achievementId, CancellationToken cancellationToken = default)
        {
            return await _context.Achievements.FindAsync([achievementId], cancellationToken);
        }

        //-----------------------UPDATE----------------------
        public async Task<Achievement?> UpdateAchievementAsync(Achievement achievement, CancellationToken cancellationToken = default)
        {
            var existingAchievement = await _context.Achievements.FindAsync([achievement.Id], cancellationToken);

            if (existingAchievement == null)
            {
                return null;
            }

            _context.Achievements.Update(achievement);
            return achievement;
        }

        //----------------------DELETE-----------------------
        public async Task<bool> DeleteAchievementAsync(int achievementId, CancellationToken cancellationToken = default)
        {
            var achievement = await _context.Achievements.FindAsync([achievementId], cancellationToken);

            if (achievement == null)
            {
                return false;
            }

            _context.Achievements.Remove(achievement);
            return true;
        }
    }
}