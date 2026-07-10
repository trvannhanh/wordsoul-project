using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class PronunciationAttemptRepository : IPronunciationAttemptRepository
    {
        private readonly WordSoulDbContext _context;

        public PronunciationAttemptRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PronunciationAttempt attempt, CancellationToken ct = default)
        {
            await _context.PronunciationAttempts.AddAsync(attempt, ct);
        }

        public async Task<List<PronunciationAttempt>> GetByUserAndVocabAsync(
            int userId,
            int vocabularyId,
            int limit = 10,
            CancellationToken ct = default)
        {
            return await _context.PronunciationAttempts
                .AsNoTracking()
                .Include(pa => pa.Vocabulary)
                .Where(pa => pa.UserId == userId && pa.VocabularyId == vocabularyId)
                .OrderByDescending(pa => pa.AttemptTime)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task<int> GetTotalPerfectCountAsync(int userId, CancellationToken ct = default)
        {
            return await _context.PronunciationAttempts
                .AsNoTracking()
                .CountAsync(pa => pa.UserId == userId && pa.Result == PronunciationResult.Perfect, ct);
        }

        public async Task<int> GetDistinctWordsPracticedAsync(int userId, CancellationToken ct = default)
        {
            return await _context.PronunciationAttempts
                .AsNoTracking()
                .Where(pa => pa.UserId == userId)
                .Select(pa => pa.VocabularyId)
                .Distinct()
                .CountAsync(ct);
        }

        public async Task<PronunciationStatsDto> GetUserStatsAsync(int userId, CancellationToken ct = default)
        {
            var stats = await _context.PronunciationAttempts
                .AsNoTracking()
                .Where(pa => pa.UserId == userId)
                .GroupBy(pa => pa.UserId)
                .Select(g => new
                {
                    TotalAttempts = g.Count(),
                    PerfectCount = g.Count(pa => pa.Result == PronunciationResult.Perfect),
                    NearMissCount = g.Count(pa => pa.Result == PronunciationResult.NearMiss),
                    WrongCount = g.Count(pa => pa.Result == PronunciationResult.Wrong),
                    DistinctWordsPracticed = g.Select(pa => pa.VocabularyId).Distinct().Count()
                })
                .FirstOrDefaultAsync(ct);

            if (stats == null)
            {
                return new PronunciationStatsDto();
            }

            return new PronunciationStatsDto
            {
                TotalAttempts = stats.TotalAttempts,
                PerfectCount = stats.PerfectCount,
                NearMissCount = stats.NearMissCount,
                WrongCount = stats.WrongCount,
                DistinctWordsPracticed = stats.DistinctWordsPracticed,
                PerfectRate = stats.TotalAttempts > 0 
                    ? Math.Round((double)stats.PerfectCount / stats.TotalAttempts * 100, 1) 
                    : 0
            };
        }
    }
}
