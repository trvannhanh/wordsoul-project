using WordSoul.Application.DTOs.Battle;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Queue ELO-based matchmaking. Thread-safe, singleton scope.
    /// </summary>
    public interface IMatchmakingQueueService
    {
        /// <summary>Thêm người chơi vào queue. Trả về queueId duy nhất.</summary>
        string Enqueue(int userId, int pvpRating, string connectionId, List<int> selectedPetIds);

        /// <summary>Rút khỏi queue. Trả false nếu không tìm thấy.</summary>
        bool Dequeue(string queueId);

        /// <summary>Rút khỏi queue theo connectionId (dùng khi disconnect).</summary>
        void DequeueByConnection(string connectionId);

        /// <summary>Tìm và tách ra tất cả các cặp có thể match. Mỗi phần tử là (entry1, entry2).</summary>
        IReadOnlyList<(MatchmakingEntry P1, MatchmakingEntry P2)> DrainMatches();
    }

    public record MatchmakingEntry(
        string QueueId,
        int UserId,
        int PvpRating,
        string ConnectionId,
        List<int> SelectedPetIds,
        DateTime JoinedAt
    );
}
