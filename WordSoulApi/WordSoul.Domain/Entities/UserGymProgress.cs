using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Theo dõi tiến trình của một User đối với một GymLeader cụ thể.
    /// Composite PK: (UserId, GymLeaderId)
    /// </summary>
    public class UserGymProgress
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public int GymLeaderId { get; set; }
        public GymLeader? GymLeader { get; set; }

        // Trạng thái: Locked → Unlocked → Defeated
        public GymStatus Status { get; set; } = GymStatus.Locked;

        // Số lần thử thách đấu tổng cộng (không giới hạn)
        public int TotalAttempts { get; set; } = 0;

        // Thời điểm thử gần nhất — dùng để tính cooldown
        public DateTime? LastAttemptAt { get; set; }

        // Khi nào đã chinh phục
        public DateTime? DefeatedAt { get; set; }

        // % đúng cao nhất đã đạt được (0–100)
        public int BestScore { get; set; } = 0;

        // Computed: còn cooldown không?
        public bool IsOnCooldown(int cooldownHours)
            => LastAttemptAt.HasValue
               && DateTime.UtcNow < LastAttemptAt.Value.AddHours(cooldownHours);

        // Computed: thời điểm cooldown hết hạn
        public DateTime? CooldownEndsAt(int cooldownHours)
            => LastAttemptAt.HasValue
               ? LastAttemptAt.Value.AddHours(cooldownHours)
               : null;
    }
}
