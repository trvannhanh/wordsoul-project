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
        private readonly IHubContext<Hubs.BattleHub> _hub;

        public MatchmakingNotifier(IHubContext<Hubs.BattleHub> hub)
            => _hub = hub;

        public Task NotifyMatchFound(string connectionId, int sessionId, CancellationToken ct = default)
            => _hub.Clients.Client(connectionId).SendAsync("MatchFound", new { sessionId }, ct);

        public Task NotifyMatchmakingError(string connectionId, string error, CancellationToken ct = default)
            => _hub.Clients.Client(connectionId).SendAsync("MatchmakingError", new { error }, ct);
    }
}
