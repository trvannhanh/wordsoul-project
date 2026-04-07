namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Abstraction để MatchmakingWorker (Infrastructure layer) notify clients
    /// mà không cần phụ thuộc trực tiếp vào WordSoul.Api (tránh circular dependency).
    /// Implementation dùng IHubContext<BattleHub> ở Api layer.
    /// </summary>
    public interface IMatchmakingNotifier
    {
        Task NotifyMatchFound(string connectionId, int sessionId, CancellationToken ct = default);
        Task NotifyMatchmakingError(string connectionId, string error, CancellationToken ct = default);
    }
}
