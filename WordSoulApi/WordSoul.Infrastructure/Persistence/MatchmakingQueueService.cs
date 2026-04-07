using System.Collections.Concurrent;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.Persistence
{
    /// <summary>
    /// ELO-based matchmaking queue dùng ConcurrentDictionary — thread-safe, single-instance.
    /// ELO range: ±200 ban đầu, mở rộng +50 mỗi 10 giây chờ.
    /// </summary>
    public class MatchmakingQueueService : IMatchmakingQueueService
    {
        // queueId → entry
        private readonly ConcurrentDictionary<string, MatchmakingEntry> _queue = new();

        public string Enqueue(int userId, int pvpRating, string connectionId, List<int> selectedPetIds)
        {
            // Loại bỏ entry cũ của cùng user nếu có
            var existing = _queue.Values.FirstOrDefault(e => e.UserId == userId);
            if (existing != null) _queue.TryRemove(existing.QueueId, out _);

            var queueId = Guid.NewGuid().ToString("N")[..12]; // 12-char ID
            var entry = new MatchmakingEntry(queueId, userId, pvpRating, connectionId, selectedPetIds, DateTime.UtcNow);
            _queue[queueId] = entry;
            return queueId;
        }

        public bool Dequeue(string queueId)
            => _queue.TryRemove(queueId, out _);

        public void DequeueByConnection(string connectionId)
        {
            var entry = _queue.Values.FirstOrDefault(e => e.ConnectionId == connectionId);
            if (entry != null) _queue.TryRemove(entry.QueueId, out _);
        }

        /// <summary>
        /// Scan queue và ghép cặp. ELO diff ≤ (200 + 50 * floor(waitSeconds / 10)).
        /// Mỗi user chỉ được ghép 1 lần. Returns danh sách (P1, P2) matched.
        /// </summary>
        public IReadOnlyList<(MatchmakingEntry P1, MatchmakingEntry P2)> DrainMatches()
        {
            var now = DateTime.UtcNow;
            var candidates = _queue.Values.OrderBy(e => e.JoinedAt).ToList();
            var matched = new HashSet<string>(); // queueIds đã được ghép
            var result = new List<(MatchmakingEntry, MatchmakingEntry)>();

            for (int i = 0; i < candidates.Count; i++)
            {
                var p1 = candidates[i];
                if (matched.Contains(p1.QueueId)) continue;

                // Expand range: +50 per 10s wait
                double waitSeconds = (now - p1.JoinedAt).TotalSeconds;
                int maxDiff = 200 + (int)(waitSeconds / 10) * 50;

                for (int j = i + 1; j < candidates.Count; j++)
                {
                    var p2 = candidates[j];
                    if (matched.Contains(p2.QueueId)) continue;
                    if (p1.UserId == p2.UserId) continue;

                    int eloDiff = Math.Abs(p1.PvpRating - p2.PvpRating);
                    if (eloDiff <= maxDiff)
                    {
                        // Ghép cặp thành công
                        matched.Add(p1.QueueId);
                        matched.Add(p2.QueueId);
                        result.Add((p1, p2));
                        break;
                    }
                }
            }

            // Xóa các entry đã ghép khỏi queue
            foreach (var (p1, p2) in result)
            {
                _queue.TryRemove(p1.QueueId, out _);
                _queue.TryRemove(p2.QueueId, out _);
            }

            return result;
        }
    }
}
