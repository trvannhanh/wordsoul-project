using Microsoft.EntityFrameworkCore;
using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.Persistence
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly WordSoulDbContext _db;

        public AdminDashboardService(WordSoulDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = DateTime.UtcNow.AddDays(-7).Date;

            var totalUsers = await _db.Users.CountAsync(ct);

            var activeUsersToday = await _db.LearningSessions
                .Where(s => s.StartTime >= today)
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync(ct);

            var totalVocabularySets = await _db.VocabularySets.CountAsync(ct);

            var totalLearningSessions = await _db.LearningSessions.CountAsync(ct);

            var newUsersThisWeek = await _db.Users
                .Where(u => u.CreatedAt >= weekAgo)
                .CountAsync(ct);

            var topXpUsers = await _db.Users
                .OrderByDescending(u => u.XP)
                .Take(5)
                .Select(u => new TopUserDto
                {
                    UserId = u.Id,
                    UserName = u.Username ?? u.Email,
                    TotalXP = u.XP,
                    TotalAP = u.AP,
                })
                .ToListAsync(ct);

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsersToday = activeUsersToday,
                TotalVocabularySets = totalVocabularySets,
                TotalLearningSessions = totalLearningSessions,
                NewUsersThisWeek = newUsersThisWeek,
                TopXpUsers = topXpUsers,
            };
        }
    }
}
