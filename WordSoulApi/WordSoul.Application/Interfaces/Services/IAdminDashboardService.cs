using WordSoul.Application.DTOs.Admin;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
        Task<PvpLeaderboardDto> GetPvpLeaderboardAsync(int top = 50, CancellationToken ct = default);
        Task<UserLearningProgressDto> GetUserLearningProgressAsync(int userId, CancellationToken ct = default);
        Task<SessionAnalyticsDto> GetSessionAnalyticsAsync(int days = 30, CancellationToken ct = default);
        Task<UserReviewHistoryPageDto> GetUserReviewHistoryAsync(
            int userId, int page, int pageSize,
            DateTime? from, DateTime? to,
            CancellationToken ct = default);
        Task<BattleSessionPageDto> GetBattleSessionsAsync(
            int page, int pageSize,
            int? userId, string? type, string? status,
            CancellationToken ct = default);
        Task<BattleReplayDto?> GetBattleReplayAsync(int sessionId, CancellationToken ct = default);
    }
}
