
using WordSoul.Application.DTOs;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeActivityLogService : IActivityLogService
    {
        public Task CreateActivityLogAsync(
            int userId,
            string action,
            string details,
            CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<List<ActivityLogDto>> GetUserActivityLogsAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            return Task.FromResult(new List<ActivityLogDto>());
        }

        public Task<List<ActivityLogDto>> GetAllActivityLogsAsync(
            string? action = null,
            DateTime? fromDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            return Task.FromResult(new List<ActivityLogDto>());
        }

        public Task TrackUserLoginAsync(int userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackUserLogoutAsync(int userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackUserRegisterAsync(int userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackStartLearningSessionAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackFinishLearningSessionAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackAnswerQuestionAsync(int userId, int vocabularyId, bool isCorrect, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackVocabularyReviewedAsync(int userId, int vocabularyId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackPetUnlockedAsync(int userId, int petId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackPetUpgradedAsync(int userId, int petId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackRewardClaimedAsync(int userId, int rewardId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackQuestClaimedAsync(int userId, int questId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackAchievementUnlockedAsync(int userId, int achievementId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackDailyStreakIncreasedAsync(int userId, int newStreakCount, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TrackDailyStreakBrokenAsync(int userId, int previousStreakCount, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
