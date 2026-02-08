using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class UserAchievementService : IUserAchievementService
    {
        private readonly IUnitOfWork _uow;

        public UserAchievementService(IUnitOfWork uow)
        {
            _uow = uow;
        }


        public async Task<List<UserAchievementDto>> GetUserAchievementsAsync(
            int userId,
            CancellationToken ct = default)
        {
            var userAchievements = await _uow.UserAchievement
                .GetUserAchievementByUserAsync(userId, ct);

            return userAchievements.Select(ua =>
            {
                var target = ua.Achievement!.ConditionValue;

                return new UserAchievementDto
                {
                    AchievementId = ua.AchievementId,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    ProgressValue = ua.ProgressValue,
                    TargetValue = target,
                    ProgressPercent = ua.GetProgressPercent(target),
                    Remaining = ua.GetRemaining(target),
                    IsCompleted = ua.IsCompleted
                };
            }).ToList();
        }

        public async Task UpdateAchievementProgressAsync(
            int userId,
            ConditionType conditionType,
            int increment,
            CancellationToken ct = default)
        {
            var achievements = await _uow.Achievement
                .GetAchievementsAsync(conditionType, 1, int.MaxValue, ct);

            foreach (var achievement in achievements)
            {
                var userAchievement = await _uow.UserAchievement
                    .GetUserAchievementAsync(userId, achievement.Id, ct);

                if (userAchievement == null || userAchievement.IsCompleted)
                    continue;

                userAchievement.ProgressValue = Math.Min(
                    userAchievement.ProgressValue + increment,
                    achievement.ConditionValue
                );

                if (userAchievement.ProgressValue >= achievement.ConditionValue)
                {
                    userAchievement.IsCompleted = true;
                    userAchievement.CompletedAt = DateTime.UtcNow;
                }

                await _uow.UserAchievement.UpdateUserAchievementAsync(userAchievement, ct);
            }

            await _uow.SaveChangesAsync(ct);
        }
    }
}
