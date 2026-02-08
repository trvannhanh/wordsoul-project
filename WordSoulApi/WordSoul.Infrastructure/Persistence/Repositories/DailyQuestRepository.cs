using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class DailyQuestRepository : IDailyQuestRepository
    {
        private readonly WordSoulDbContext _context;

        public DailyQuestRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task<List<DailyQuest>> GetActiveQuestsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.DailyQuests
                .AsNoTracking()
                .Where(q => q.IsActive)
                .OrderBy(q => q.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<DailyQuest?> GetByIdAsync(
            int questId,
            CancellationToken cancellationToken = default)
        {
            return await _context.DailyQuests.FindAsync(
                new object[] { questId },
                cancellationToken);
        }
    }
}
