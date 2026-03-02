using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class UserDailyQuestRepository : IUserDailyQuestRepository
    {
        private readonly WordSoulDbContext _context;

        public UserDailyQuestRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task<UserDailyQuest?> GetUserDailyQuestAsync(
            int userId,
            int dailyQuestId,
            DateTime questDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserDailyQuests
                .FirstOrDefaultAsync(
                    x => x.UserId == userId
                      && x.DailyQuestId == dailyQuestId
                      && x.QuestDate == questDate.Date,
                    cancellationToken);
        }

        public async Task<List<UserDailyQuest>> GetUserDailyQuestsByUserAndDateAsync(
            int userId,
            DateTime questDate,
            CancellationToken cancellationToken = default)
        {
            var start = questDate.Date;
            var end = start.AddDays(1);

            return await _context.UserDailyQuests
                .Include(x => x.DailyQuest)
                .Where(x => x.UserId == userId
                         && x.QuestDate >= start
                         && x.QuestDate < end)
                .ToListAsync(cancellationToken);
        }

        public Task<UserDailyQuest> CreateUserDailyQuestAsync(
            UserDailyQuest userDailyQuest,
            CancellationToken cancellationToken = default)
        {
            _context.UserDailyQuests.Add(userDailyQuest);
            return Task.FromResult(userDailyQuest);
        }

        public Task UpdateUserDailyQuestAsync(
            UserDailyQuest userDailyQuest,
            CancellationToken cancellationToken = default)
        {
            _context.UserDailyQuests.Update(userDailyQuest);
            return Task.CompletedTask;
        }

        public async Task<UserDailyQuest?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.UserDailyQuests
                .Include(x => x.DailyQuest)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }
    }
}
