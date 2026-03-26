using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Gym;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Enums;

namespace WordSoul.Infrastructure.Persistence
{
    /// <summary>
    /// Quản lý tiến trình Gym Leader: kiểm tra điều kiện mở khóa và trả về thông tin Gym kèm trạng thái user.
    /// </summary>
    public class GymLeaderService : IGymLeaderService
    {
        private readonly WordSoulDbContext _db;
        private readonly ILogger<GymLeaderService> _logger;

        public GymLeaderService(WordSoulDbContext db, ILogger<GymLeaderService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<GymLeaderDto>> GetAllGymsForUserAsync(int userId, CancellationToken ct = default)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, ct)
                ?? throw new KeyNotFoundException($"User {userId} not found");

            var gymLeaders = await _db.GymLeaders
                .AsNoTracking()
                .OrderBy(gl => gl.GymOrder)
                .ToListAsync(ct);

            var userProgresses = await _db.UserGymProgresses
                .AsNoTracking()
                .Where(ugp => ugp.UserId == userId)
                .ToListAsync(ct);

            var result = new List<GymLeaderDto>();

            foreach (var gym in gymLeaders)
            {
                var progress = userProgresses.FirstOrDefault(ugp => ugp.GymLeaderId == gym.Id);
                var vocabCount = await CountEligibleVocabsAsync(userId, gym, ct);
                result.Add(MapToDto(gym, progress, user.XP, vocabCount));
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<GymLeaderDto?> GetGymDetailAsync(int userId, int gymId, CancellationToken ct = default)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null) return null;

            var gym = await _db.GymLeaders.AsNoTracking().FirstOrDefaultAsync(gl => gl.Id == gymId, ct);
            if (gym == null) return null;

            var progress = await _db.UserGymProgresses
                .AsNoTracking()
                .FirstOrDefaultAsync(ugp => ugp.UserId == userId && ugp.GymLeaderId == gymId, ct);

            var vocabCount = await CountEligibleVocabsAsync(userId, gym, ct);
            return MapToDto(gym, progress, user.XP, vocabCount);
        }

        /// <inheritdoc />
        public async Task CheckAndUnlockGymsAsync(int userId, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null) return;

            var gymLeaders = await _db.GymLeaders.OrderBy(gl => gl.GymOrder).ToListAsync(ct);
            var userProgresses = await _db.UserGymProgresses.Where(ugp => ugp.UserId == userId).ToListAsync(ct);

            bool changed = false;

            foreach (var gym in gymLeaders)
            {
                var progress = userProgresses.FirstOrDefault(ugp => ugp.GymLeaderId == gym.Id);

                if (progress?.Status == GymStatus.Defeated) continue;
                if (progress?.Status == GymStatus.Unlocked) continue;

                // Gym trước phải đã Defeated
                if (gym.GymOrder > 1)
                {
                    var prevGym = gymLeaders.First(gl => gl.GymOrder == gym.GymOrder - 1);
                    var prevProgress = userProgresses.FirstOrDefault(ugp => ugp.GymLeaderId == prevGym.Id);
                    if (prevProgress?.Status != GymStatus.Defeated) continue;
                }

                if (user.XP < gym.XpThreshold) continue;

                var vocabCount = await CountEligibleVocabsAsync(userId, gym, ct);
                if (vocabCount < gym.VocabThreshold) continue;

                // Unlock!
                if (progress == null)
                {
                    _db.UserGymProgresses.Add(new Domain.Entities.UserGymProgress
                    {
                        UserId = userId,
                        GymLeaderId = gym.Id,
                        Status = GymStatus.Unlocked
                    });
                }
                else
                {
                    progress.Status = GymStatus.Unlocked;
                }

                _logger.LogInformation("User {UserId} unlocked Gym {GymOrder} - {GymName}", userId, gym.GymOrder, gym.Name);
                changed = true;
            }

            if (changed) await _db.SaveChangesAsync(ct);
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        private async Task<int> CountEligibleVocabsAsync(
            int userId,
            Domain.Entities.GymLeader gym,
            CancellationToken ct)
        {
            var eligibleStates = gym.RequiredMemoryState switch
            {
                "Review" => new[] { "Review", "Mastered" },
                _ => new[] { "Learning", "Review", "Mastered" }
            };

            return await _db.UserVocabularyProgresses
                .AsNoTracking()
                .Where(uvp =>
                    uvp.UserId == userId &&
                    eligibleStates.Contains(uvp.MemoryState) &&
                    uvp.Vocabulary!.CEFRLevel == gym.RequiredCefrLevel)
                .CountAsync(ct);
        }

        private static GymLeaderDto MapToDto(
            Domain.Entities.GymLeader gym,
            Domain.Entities.UserGymProgress? progress,
            int userXp,
            int currentVocabCount)
        {
            bool isOnCooldown = progress?.IsOnCooldown(gym.CooldownHours) ?? false;
            DateTime? cooldownEndsAt = progress?.CooldownEndsAt(gym.CooldownHours);

            return new GymLeaderDto
            {
                Id = gym.Id,
                GymOrder = gym.GymOrder,
                Name = gym.Name,
                Title = gym.Title,
                Description = gym.Description,
                AvatarUrl = gym.AvatarUrl,
                BadgeName = gym.BadgeName,
                BadgeImageUrl = gym.BadgeImageUrl,
                Theme = gym.Theme.ToString(),
                RequiredCefrLevel = gym.RequiredCefrLevel.ToString(),
                XpThreshold = gym.XpThreshold,
                VocabThreshold = gym.VocabThreshold,
                RequiredMemoryState = gym.RequiredMemoryState,
                QuestionCount = gym.QuestionCount,
                PassRatePercent = gym.PassRatePercent,
                XpReward = gym.XpReward,
                Status = progress?.Status ?? GymStatus.Locked,
                TotalAttempts = progress?.TotalAttempts ?? 0,
                BestScore = progress?.BestScore ?? 0,
                DefeatedAt = progress?.DefeatedAt,
                IsOnCooldown = isOnCooldown,
                CooldownEndsAt = isOnCooldown ? cooldownEndsAt : null,
                CurrentXp = userXp,
                CurrentVocabCount = currentVocabCount,
            };
        }
    }
}
