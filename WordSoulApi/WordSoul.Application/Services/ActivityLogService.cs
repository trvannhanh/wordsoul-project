using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WordSoul.Application.Common.Constants;
using WordSoul.Application.DTOs;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(IUnitOfWork uow, IMemoryCache cache, ILogger<ActivityLogService> logger)
        {
            _uow = uow;
            _cache = cache;
            _logger = logger;
        }

        // -------------------------------------CREATE-----------------------------------------

        public async Task CreateActivityLogAsync(
            int userId,
            string action,
            string details,
            CancellationToken ct = default)
        {
            if (userId <= 0)
                throw new ArgumentException("userId must be greater than 0.");

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("action cannot be null or empty.");

            const int maxRetry = 3;
            int attempt = 0;

            while (true)
            {
                try
                {
                    attempt++;

                    _logger.LogInformation(
                        "Tracking event {Action} for User {UserId} (Attempt {Attempt})",
                        action, userId, attempt);

                    var activityLog = new ActivityLog
                    {
                        UserId = userId,
                        Action = action,
                        Details = details,
                        Timestamp = DateTime.UtcNow
                    };

                    await _uow.ActivityLog.CreateActivityLogAsync(activityLog, ct);
                    await _uow.SaveChangesAsync(ct);

                    _logger.LogInformation(
                        "Event {Action} for User {UserId} saved successfully",
                        action, userId);

                    _cache.Remove($"ActivityLogs_User_{userId}_1_10");

                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to save event {Action} for User {UserId}, Attempt {Attempt}",
                        action, userId, attempt);

                    if (attempt >= maxRetry)
                    {
                        _logger.LogError(ex,
                            "Event tracking permanently failed for User {UserId}, Action {Action}",
                            userId, action);

                        // Không throw để tránh làm crash luồng chính
                        return;
                    }

                    await Task.Delay(200 * attempt, ct);
                }
            }
        }

        public Task TrackUserLoginAsync(int userId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.UserLogin,
                "User logged in",
                ct);
        }

        public Task TrackUserLogoutAsync(int userId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.UserLogout,
                "User logged out",
                ct);
        }

        public Task TrackUserRegisterAsync(int userId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.UserRegister,
                "User registered an account",
                ct);
        }

        public Task TrackStartLearningSessionAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.StartLearningSession,
                $"Started session {sessionId}",
                ct);
        }

        public Task TrackFinishLearningSessionAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.FinishLearningSession,
                $"Finished session {sessionId}",
                ct);
        }

        public Task TrackAnswerQuestionAsync(int userId, int vocabularyId, bool isCorrect, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.AnswerQuestion,
                $"VocabularyId={vocabularyId}, Correct={isCorrect}",
                ct);
        }


        public Task TrackVocabularyReviewedAsync(int userId, int vocabularyId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.VocabularyReviewed,
                $"VocabularyId={vocabularyId}", ct);
        }

        public Task TrackPetUnlockedAsync(int userId, int petId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.PetUnlocked,
                $"PetId={petId}",
                ct);
        }

        public Task TrackPetUpgradedAsync(int userId, int petId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.PetUpgraded,
                $"PetId={petId}",
                ct);
        }

        public Task TrackRewardClaimedAsync(int userId, int rewardId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.RewardClaimed,
                $"RewardId={rewardId}",
                ct);
        }

        public Task TrackQuestClaimedAsync(int userId, int questId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.QuestClaimed,
                $"QuestId={questId}",
                ct);
        }

        public Task TrackAchievementUnlockedAsync(int userId, int achievementId, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.AchievementUnlocked,
                $"AchievementId={achievementId}",
                ct);
        }

        public Task TrackDailyStreakIncreasedAsync(int userId, int newStreakCount, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.DailyStreakIncreased,
                $"NewStreakCount={newStreakCount}",
                ct);
        }

        public Task TrackDailyStreakBrokenAsync(int userId, int previousStreakCount, CancellationToken ct = default)
        {
            return CreateActivityLogAsync(
                userId,
                ActivityActions.DailyStreakBroken,
                $"PreviousStreakCount={previousStreakCount}",
                ct);
        }


        // -------------------------------------READ-------------------------------------------

        public async Task<List<ActivityLogDto>> GetUserActivityLogsAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");

            var cacheKey = $"ActivityLogs_User_{userId}_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out List<ActivityLogDto> cached))
            {
                _logger.LogInformation(
                    "Retrieved activity logs for user {UserId} from cache (page={Page}, size={Size})",
                    userId, pageNumber, pageSize);

                return cached;
            }

            try
            {
                _logger.LogInformation(
                    "Retrieving activity logs for user {UserId}, page={Page}, size={Size}",
                    userId, pageNumber, pageSize);

                var logs = await _uow.ActivityLog
                    .GetActivityLogsByUserIdAsync(userId, pageNumber, pageSize, ct);

                var dtos = logs.Select(MapToDto).ToList();

                _cache.Set(
                    cacheKey,
                    dtos,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving activity logs for user {UserId}, page={Page}, size={Size}",
                    userId, pageNumber, pageSize);

                throw new Exception(
                    $"Error retrieving activity logs for user {userId}: {ex.Message}",
                    ex);
            }
        }

        public async Task<List<ActivityLogDto>> GetAllActivityLogsAsync(
            string? action = null,
            DateTime? fromDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");
            if (pageSize > 100)
            {
                _logger.LogWarning("pageSize {PageSize} exceeds maximum of 100. Forcing to 100.", pageSize);
                pageSize = 100;
            }

            var cacheKey =
                $"ActivityLogs_All_{action ?? "null"}_{fromDate?.ToString("yyyyMMdd") ?? "null"}_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out List<ActivityLogDto> cached))
            {
                _logger.LogInformation(
                    "Retrieved all activity logs from cache, action={Action}, fromDate={FromDate}, page={Page}, size={Size}",
                    action ?? "null",
                    fromDate?.ToString("yyyy-MM-dd") ?? "null",
                    pageNumber,
                    pageSize);

                return cached;
            }

            try
            {
                _logger.LogInformation(
                    "Retrieving all activity logs, action={Action}, fromDate={FromDate}, page={Page}, size={Size}",
                    action ?? "null",
                    fromDate?.ToString("yyyy-MM-dd") ?? "null",
                    pageNumber,
                    pageSize);

                var logs = await _uow.ActivityLog
                    .GetAllActivityLogsAsync(action, fromDate, pageNumber, pageSize, ct);

                var dtos = logs.Select(MapToDto).ToList();

                _cache.Set(
                    cacheKey,
                    dtos,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving all activity logs, action={Action}, fromDate={FromDate}, page={Page}, size={Size}",
                    action ?? "null",
                    fromDate?.ToString("yyyy-MM-dd") ?? "null",
                    pageNumber,
                    pageSize);

                throw new Exception($"Error retrieving all activity logs: {ex.Message}", ex);
            }
        }

        // --------------------------HELPER-------------------------------------

        private ActivityLogDto MapToDto(ActivityLog log)
        {
            return new ActivityLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Username = log.User?.Username ?? "Unknown",
                Action = log.Action,
                Details = log.Details,
                Timestamp = log.Timestamp
            };
        }
    }
}
