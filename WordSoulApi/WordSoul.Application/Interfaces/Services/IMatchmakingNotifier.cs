namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Abstraction để MatchmakingWorker (Infrastructure layer) notify clients
    /// mà không cần phụ thuộc trực tiếp vào WordSoul.Api (tránh circular dependency).
    /// Implementation dùng IHubContext<BattleHub> ở Api layer.
    /// </summary>
     
    // Giải quyết vấn đề: MatchmakingWorker (Infrastructure) không được phép gọi trực tiếp BattleHub (Api)
    // Bằng cách định nghĩa interface ở Application layer, ta tạo ra một "contract" mà cả hai layer đều có thể hiểu.
    // MatchmakingWorker chỉ cần biết "tôi cần gửi thông báo" (interface), không cần biết "gửi bằng cách nào" (HubContext).
    // Việc gửi thực tế được thực hiện bởi MatchmakingNotifier (Api layer), giúp tách biệt logic.
    
    public interface IMatchmakingNotifier
    {
        Task NotifyMatchFound(string connectionId, int sessionId, CancellationToken ct = default);
        Task NotifyMatchmakingError(string connectionId, string error, CancellationToken ct = default);
    }
}
