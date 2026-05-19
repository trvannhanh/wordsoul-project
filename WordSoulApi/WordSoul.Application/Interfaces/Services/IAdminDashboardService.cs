using WordSoul.Application.DTOs.Admin;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
        Task<PvpLeaderboardDto> GetPvpLeaderboardAsync(int top = 50, CancellationToken ct = default);
    }
}
