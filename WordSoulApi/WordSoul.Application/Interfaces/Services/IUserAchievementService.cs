using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Achievement;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IUserAchievementService
    {
        Task<List<UserAchievementDto>> GetUserAchievementsAsync(
            int userId,
            CancellationToken ct = default);

        Task UpdateAchievementProgressAsync(
            int userId,
            ConditionType conditionType,
            int increment,
            CancellationToken ct = default);

        Task CheckAndUnlockAchievementsAsync(
            int userId,
            CancellationToken ct = default);
        Task ClaimAchievementRewardAsync(
            int userId,
            int achievementId,
            CancellationToken ct = default);
    }
}
