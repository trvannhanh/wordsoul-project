using WordSoul.Application.DTOs.Achievement;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.Domain.Exceptions;

namespace WordSoul.Application.Services
{
    public class UserAchievementService : IUserAchievementService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserInventoryService _inventoryService;

        public UserAchievementService(IUnitOfWork uow, IUserInventoryService userInventoryService)
        {
            _uow = uow;
            _inventoryService = userInventoryService;
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
            await using var transaction = await _uow.BeginTransactionAsync(ct);
            try
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

                await CheckAndUnlockAchievementsAsync(userId, ct);
                await _uow.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
            

           
        }

        public async Task CheckAndUnlockAchievementsAsync(
            int userId,
            CancellationToken ct = default)
        {
            var userAchievements = await _uow.UserAchievement
                .GetUserAchievementByUserAsync(userId, ct);

            foreach (var ua in userAchievements)
            {
                if (ua.IsCompleted)
                    continue;

                var target = ua.Achievement!.ConditionValue;

                if (ua.ProgressValue >= target)
                {
                    ua.IsCompleted = true;
                    ua.CompletedAt = DateTime.UtcNow;

                    await _uow.UserAchievement.UpdateUserAchievementAsync(ua, ct);
                }
            }

            await _uow.SaveChangesAsync(ct);
        }

        public async Task ClaimAchievementRewardAsync(
            int userId,
            int achievementId,
            CancellationToken ct = default)
        {
            var ua = await _uow.UserAchievement
                .GetUserAchievementAsync(userId, achievementId, ct);

            if (ua == null)
                throw new NotFoundException(nameof(UserAchievement), achievementId);

            if (!ua.IsCompleted)
                throw new InvalidOperationException("Achievement not completed.");

            if (ua.IsClaimed)
                throw new InvalidOperationException("Reward already claimed.");

            var rewardItemId = ua.Achievement!.RewardItemId;

            await _inventoryService.AddItemToUserAsync(userId, rewardItemId, 1, ct);

            ua.IsClaimed = true;
            ua.ClaimedAt = DateTime.UtcNow;

            await _uow.UserAchievement.UpdateUserAchievementAsync(ua, ct);
            await _uow.SaveChangesAsync(ct);
        }

    }
}
