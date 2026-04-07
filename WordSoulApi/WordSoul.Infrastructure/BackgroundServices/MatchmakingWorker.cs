using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Battle;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background worker chạy mỗi 5 giây, quét matchmaking queue,
    /// ghép cặp ELO-based, tạo PvP session và notify clients qua SignalR.
    /// </summary>
    public class MatchmakingWorker : BackgroundService
    {
        private readonly IMatchmakingQueueService _queue;
        private readonly IMatchmakingNotifier _notifier;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MatchmakingWorker> _logger;
        private static readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

        public MatchmakingWorker(
            IMatchmakingQueueService queue,
            IMatchmakingNotifier notifier,
            IServiceScopeFactory scopeFactory,
            ILogger<MatchmakingWorker> logger)
        {
            _queue = queue;
            _notifier = notifier;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MatchmakingWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_interval, stoppingToken);

                try
                {
                    var matches = _queue.DrainMatches();
                    if (matches.Count == 0) continue;

                    _logger.LogInformation("Matchmaking: found {Count} pair(s).", matches.Count);

                    foreach (var (p1, p2) in matches)
                        await ProcessMatchAsync(p1, p2, stoppingToken);
                }
                catch (OperationCanceledException) { /* shutdown */ }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MatchmakingWorker encountered an error.");
                }
            }
        }

        private async Task ProcessMatchAsync(MatchmakingEntry p1, MatchmakingEntry p2, CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var arena = scope.ServiceProvider.GetRequiredService<IArenaBattleService>();

                // P1 tạo session (Challenger)
                var room = await arena.CreatePvpSessionAsync(
                    new CreatePvpSessionDto { SelectedPetIds = p1.SelectedPetIds }, p1.UserId, ct);

                // P2 join session (Opponent)
                await arena.JoinPvpSessionAsync(
                    new JoinPvpSessionDto { RoomCode = room.RoomCode, SelectedPetIds = p2.SelectedPetIds },
                    p2.UserId, ct);

                _logger.LogInformation(
                    "Matched U{P1} (ELO {R1}) vs U{P2} (ELO {R2}) → session {S}",
                    p1.UserId, p1.PvpRating, p2.UserId, p2.PvpRating, room.SessionId);

                // Notify cả 2 client qua SignalR
                await _notifier.NotifyMatchFound(p1.ConnectionId, room.SessionId, ct);
                await _notifier.NotifyMatchFound(p2.ConnectionId, room.SessionId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process match U{P1} vs U{P2}.", p1.UserId, p2.UserId);
                await _notifier.NotifyMatchmakingError(p1.ConnectionId, "Matchmaking failed. Please try again.", ct);
                await _notifier.NotifyMatchmakingError(p2.ConnectionId, "Matchmaking failed. Please try again.", ct);
            }
        }
    }
}
