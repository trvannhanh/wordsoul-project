namespace WordSoul.Domain.Entities
{
    /// <summary>
    /// Trạng thái HP của từng Pokémon trong một trận Battle.
    /// Mỗi trận có 6 bản ghi (3 của P1, 3 của P2).
    /// </summary>
    public class BattlePetState
    {
        public int Id { get; set; }

        public int BattleSessionId { get; set; }
        public BattleSession? BattleSession { get; set; }

        /// <summary>1 = Challenger (người chơi), 2 = Opponent (bot / PvP player 2).</summary>
        public int PlayerIndex { get; set; }

        /// <summary>Vị trí trong đội (0 = đầu tiên, 2 = cuối).</summary>
        public int SlotIndex { get; set; }

        // ── Nguồn Pokémon ──────────────────────────────────
        /// <summary>
        /// Nếu P1 (người chơi thật): FK đến UserOwnedPet.
        /// Nếu P2 (Bot PvE): null, dùng GymLeaderPetId.
        /// </summary>
        public int? UserOwnedPetId { get; set; }
        public UserOwnedPet? UserOwnedPet { get; set; }

        /// <summary>Nếu Bot: FK đến GymLeaderPet.</summary>
        public int? GymLeaderPetId { get; set; }
        public GymLeaderPet? GymLeaderPet { get; set; }

        /// <summary>Tên hiển thị (lấy từ Pet.Name hoặc Nickname).</summary>
        public string DisplayName { get; set; } = "";
        public string? ImageUrl { get; set; }

        public WordSoul.Domain.Enums.PetType PetType { get; set; } = WordSoul.Domain.Enums.PetType.Normal;
        public WordSoul.Domain.Enums.PetType? SecondaryPetType { get; set; }

        // ── Trạng thái HP ─────────────────────────────────
        public int MaxHp { get; set; } = 100;
        public int CurrentHp { get; set; } = 100;
        public bool IsFainted { get; set; } = false;
        public DateTime? FaintedAt { get; set; }
    }
}
