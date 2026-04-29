using Microsoft.AspNetCore.SignalR;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Services
{
    /// <summary>
    /// Implement IMatchmakingNotifier dùng IHubContext<BattleHub>.
    /// Được đăng ký trong Api layer, inject vào MatchmakingWorker qua interface.
    /// </summary>
    public class MatchmakingNotifier : IMatchmakingNotifier
    {
        // Lấy thông tin về HubContext của BattleHub
        private readonly IHubContext<Hubs.BattleHub> _hub;

        // Constructor để inject HubContext
        public MatchmakingNotifier(IHubContext<Hubs.BattleHub> hub)
            => _hub = hub;

        // Gửi thông báo tìm thấy trận đấu
        public Task NotifyMatchFound(string connectionId, int sessionId, CancellationToken ct = default)
            => _hub.Clients.Client(connectionId).SendAsync("MatchFound", new { sessionId }, ct);

        // Gửi thông báo lỗi matchmaking
        public Task NotifyMatchmakingError(string connectionId, string error, CancellationToken ct = default)
            => _hub.Clients.Client(connectionId).SendAsync("MatchmakingError", new { error }, ct);
    }
}
