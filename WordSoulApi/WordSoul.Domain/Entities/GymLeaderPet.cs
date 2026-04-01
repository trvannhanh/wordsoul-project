namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Một trong 3 Pokémon được gắn với Gym Leader để chiến đấu.
    /// Mỗi GymLeader có đúng 3 GymLeaderPet (SlotIndex 0, 1, 2).
    /// </summary>
    public class GymLeaderPet
    {
        public int Id { get; set; }

        public int GymLeaderId { get; set; }
        public GymLeader? GymLeader { get; set; }

        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        /// <summary>Vị trí trong đội (0 = đầu, 1 = giữa, 2 = cuối).</summary>
        public int SlotIndex { get; set; }

        /// <summary>Tên hiển thị (có thể override tên Pet mặc định).</summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// Độ chính xác trả lời của Bot PvE (0.0 – 1.0).
        /// Gym cao hơn thì Bot chính xác hơn và nhanh hơn.
        /// </summary>
        public double BotAccuracy { get; set; } = 0.6;

        /// <summary>Tốc độ trả lời trung bình của Bot (ms). Nhỏ hơn = nhanh hơn.</summary>
        public int BotAvgResponseMs { get; set; } = 5000;

        /// <summary>Level của Bot Pet dùng để tính HP.</summary>
        public int Level { get; set; } = 10;
    }
}
