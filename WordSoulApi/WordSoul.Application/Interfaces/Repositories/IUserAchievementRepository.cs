using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserAchievementRepository
    {

        Task<UserAchievement?> GetUserAchievementAsync(
        int userId,
        int achievementId,
        CancellationToken ct = default);

        Task<List<UserAchievement>> GetUserAchievementByUserAsync(
            int userId,
            CancellationToken ct = default);


        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Tạo một bản ghi thành tựu mới cho người dùng.
        /// </summary>
        Task CreateUserAchievementAsync(
            UserAchievement userAchievement,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạo đồng thời nhiều bản ghi thành tựu cho người dùng (dùng khi unlock nhiều achievement cùng lúc).
        /// </summary>
        Task BulkCreateUserAchievementAsync(
            IEnumerable<UserAchievement> userAchievements,
            CancellationToken cancellationToken = default);

        Task UpdateUserAchievementAsync(
            UserAchievement userAchievement,
            CancellationToken ct = default);
    }
}