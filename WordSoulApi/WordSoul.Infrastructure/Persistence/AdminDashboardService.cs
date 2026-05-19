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

        public async Task<SessionAnalyticsDto> GetSessionAnalyticsAsync(int days = 30, CancellationToken ct = default)
        {
            var since = DateTime.UtcNow.AddDays(-days).Date;

            // Load sessions in range with answer records
            var sessions = await _db.LearningSessions
                .Where(s => s.StartTime >= since)
                .Include(s => s.User)
                .Include(s => s.AnswerRecords)
                .ToListAsync(ct);

            var totalSessions     = sessions.Count;
            var learningSessions  = sessions.Count(s => s.Type == WordSoul.Domain.Enums.SessionType.Learning);
            var reviewSessions    = sessions.Count(s => s.Type == WordSoul.Domain.Enums.SessionType.Review);
            var completedSessions = sessions.Count(s => s.IsCompleted);
            var completionRate    = totalSessions == 0 ? 0 :
                Math.Round((double)completedSessions / totalSessions * 100, 1);

            var allAnswers   = sessions.SelectMany(s => s.AnswerRecords).ToList();
            var totalAnswers  = allAnswers.Count;
            var correctAnswers = allAnswers.Count(a => a.IsCorrect);
            var overallAccuracy = totalAnswers == 0 ? 0 :
                Math.Round((double)correctAnswers / totalAnswers * 100, 1);
            var avgResponseTime = totalAnswers == 0 ? 0 :
                Math.Round(allAnswers.Average(a => a.ResponseTimeSeconds), 2);
            var totalHints = allAnswers.Sum(a => a.HintCount);

            // Daily stats
            var dailyGroups = sessions
                .GroupBy(s => s.StartTime.Date)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var dayAnswers = g.SelectMany(s => s.AnswerRecords).ToList();
                    return new DailySessionStatDto
                    {
                        Date             = g.Key.ToString("yyyy-MM-dd"),
                        LearningSessions = g.Count(s => s.Type == WordSoul.Domain.Enums.SessionType.Learning),
                        ReviewSessions   = g.Count(s => s.Type == WordSoul.Domain.Enums.SessionType.Review),
                        CorrectAnswers   = dayAnswers.Count(a => a.IsCorrect),
                        TotalAnswers     = dayAnswers.Count,
                    };
                })
                .ToList();

            // Top 5 active users
            var topUsers = sessions
                .Where(s => s.IsCompleted && s.User != null)
                .GroupBy(s => new { s.UserId, UserName = s.User!.Username ?? s.User.Email })
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g =>
                {
                    var userAnswers = g.SelectMany(s => s.AnswerRecords).ToList();
                    return new ActiveUserStatDto
                    {
                        UserId        = g.Key.UserId,
                        UserName      = g.Key.UserName,
                        SessionCount  = g.Count(),
                        CorrectAnswers = userAnswers.Count(a => a.IsCorrect),
                    };
                })
                .ToList();

            return new SessionAnalyticsDto
            {
                TotalSessions        = totalSessions,
                LearningSessions     = learningSessions,
                ReviewSessions       = reviewSessions,
                CompletedSessions    = completedSessions,
                CompletionRate       = completionRate,
                TotalAnswers         = totalAnswers,
                CorrectAnswers       = correctAnswers,
                OverallAccuracy      = overallAccuracy,
                AvgResponseTimeSeconds = avgResponseTime,
                TotalHintsUsed       = totalHints,
                DailyStats           = dailyGroups,
                TopActiveUsers       = topUsers,
            };
        }

        public async Task<UserLearningProgressDto> GetUserLearningProgressAsync(int userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var since30d = now.AddDays(-30);

            var progress = await _db.UserVocabularyProgresses
                .Where(p => p.UserId == userId)
                .Include(p => p.Vocabulary)
                .ToListAsync(ct);

            var newCount      = progress.Count(p => p.MemoryState == "New");
            var learningCount = progress.Count(p => p.MemoryState == "Learning");
            var reviewCount   = progress.Count(p => p.MemoryState == "Review");
            var masteredCount = progress.Count(p => p.MemoryState == "Mastered");

            var dueCount      = progress.Count(p => p.NextReviewTime <= now);
            var nextReview    = progress
                .Where(p => p.NextReviewTime > now)
                .Select(p => (DateTime?)p.NextReviewTime)
                .OrderBy(d => d)
                .FirstOrDefault();

            var totalCorrect = progress.Sum(p => p.CorrectCount);
            var totalWrong   = progress.Sum(p => p.WrongCount);
            var totalAnswers = totalCorrect + totalWrong;
            var accuracyRate = totalAnswers == 0 ? 0 :
                Math.Round((double)totalCorrect / totalAnswers * 100, 1);

            var avgRetention = progress.Count == 0 ? 0 :
                Math.Round((double)progress.Average(p => (double)p.RetentionScore), 1);

            var struggleWords = progress
                .Where(p => p.WrongCount > 0)
                .OrderByDescending(p => p.WrongCount)
                .Take(10)
                .Select(p => new StruggleWordEntry
                {
                    Word           = p.Vocabulary?.Word ?? string.Empty,
                    Meaning        = p.Vocabulary?.Meaning,
                    WrongCount     = p.WrongCount,
                    RetentionScore = (double)p.RetentionScore,
                })
                .ToList();

            var sessions = await _db.LearningSessions
                .Where(s => s.UserId == userId && s.StartTime >= since30d)
                .ToListAsync(ct);

            return new UserLearningProgressDto
            {
                UserId                = userId,
                NewCount              = newCount,
                LearningCount         = learningCount,
                ReviewCount           = reviewCount,
                MasteredCount         = masteredCount,
                TotalVocabularies     = progress.Count,
                DueForReviewCount     = dueCount,
                NextReviewTime        = nextReview,
                TotalCorrect          = totalCorrect,
                TotalWrong            = totalWrong,
                AccuracyRate          = accuracyRate,
                AverageRetentionScore = avgRetention,
                TotalSessions         = sessions.Count,
                CompletedSessions     = sessions.Count(s => s.IsCompleted),
                StruggleWords         = struggleWords,
            };
        }

        public async Task<PvpLeaderboardDto> GetPvpLeaderboardAsync(int top = 50, CancellationToken ct = default)
        {
            var activePlayers = await _db.Users
                .Where(u => u.PvpWins > 0 || u.PvpLosses > 0)
                .CountAsync(ct);

            var avgRating = activePlayers == 0 ? 0 :
                await _db.Users
                    .Where(u => u.PvpWins > 0 || u.PvpLosses > 0)
                    .AverageAsync(u => (double)u.PvpRating, ct);

            var highest = activePlayers == 0 ? 0 :
                await _db.Users
                    .Where(u => u.PvpWins > 0 || u.PvpLosses > 0)
                    .MaxAsync(u => u.PvpRating, ct);

            var entries = await _db.Users
                .OrderByDescending(u => u.PvpRating)
                .ThenByDescending(u => u.PvpWins)
                .Take(top)
                .Select(u => new PvpLeaderboardEntryDto
                {
                    UserId = u.Id,
                    UserName = u.Username ?? u.Email,
                    AvatarUrl = u.AvatarUrl,
                    PvpRating = u.PvpRating,
                    Wins = u.PvpWins,
                    Losses = u.PvpLosses,
                })
                .ToListAsync(ct);

            for (int i = 0; i < entries.Count; i++)
                entries[i].Rank = i + 1;

            return new PvpLeaderboardDto
            {
                Entries = entries,
                TotalActivePlayers = activePlayers,
                AverageRating = Math.Round(avgRating, 0),
                HighestRating = highest,
            };
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
