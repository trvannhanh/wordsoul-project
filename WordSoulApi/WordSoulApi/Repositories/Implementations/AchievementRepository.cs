using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class AchievementRepository : IAchievementRepository
    {
        private readonly WordSoulDbContext _context;

        public AchievementRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //-----------------------CREATE-------------------
        public async Task CreateAchievementAsync(Achievement achievement)
        {
            await _context.Achievements.AddAsync(achievement);
            await _context.SaveChangesAsync();
        }

        //-----------------------READ---------------------
        public async Task<List<Achievement>> GetAchievementsAsync(ConditionType? conditionType, int pageNumber, int pageSize)
        {
            var query = _context.Achievements
                .AsNoTracking()
                .AsQueryable();

            if (conditionType.HasValue)
                query = query.Where(a =>  a.ConditionType == conditionType);

            return await query
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync();
        }

        public async Task<Achievement?> GetAchievementByIdAsync(int achievementId)
        {
            return await _context.Achievements.FindAsync(achievementId);
        }

        //-----------------------UPDATE----------------------
        public async Task<Achievement> UpdateAchievementAsync(Achievement achievement)
        {
            var existingAchievement = await _context.Achievements.FindAsync(achievement);
            {
                _context.Achievements.Update(achievement);
                await _context.SaveChangesAsync();
                return achievement;
            }
        }

        //----------------------DELETE-----------------------
        public async Task DeleteAchievementAsync(int achievementId)
        {
            var achievement = await _context.Achievements.FindAsync(achievementId);
            if (achievement != null)
            {
                _context.Achievements.Remove(achievement);
                await _context.SaveChangesAsync();
            }
        }
    }
}
