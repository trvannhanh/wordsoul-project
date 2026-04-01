namespace WordSoul.Application.DTOs.Battle
{
    // ── Inbound (Client → REST API) ────────────────────────────────────────────

    public class CreatePvpSessionDto
    {
        /// <summary>3 UserOwnedPet IDs người chơi chọn.</summary>
        public List<int> SelectedPetIds { get; set; } = [];
    }

    public class JoinPvpSessionDto
    {
        public string RoomCode { get; set; } = "";
        /// <summary>3 UserOwnedPet IDs của người join.</summary>
        public List<int> SelectedPetIds { get; set; } = [];
    }

    // ── Outbound (Server → Client) ─────────────────────────────────────────────

    public class PvpRoomCreatedDto
    {
        public int SessionId { get; set; }
        public string RoomCode { get; set; } = "";
    }

    /// <summary>Broadcast sang P1 khi P2 join phòng (qua REST API).</summary>
    public class OpponentJoinedDto
    {
        public string OpponentName { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public int OpponentRating { get; set; }
    }

    /// <summary>Thông tin ELO thay đổi sau PvP, đính kèm vào BattleEndedDto.</summary>
    public class PvpEloResultDto
    {
        public int RatingChange { get; set; }   // dương = tăng, âm = giảm
        public int NewRating { get; set; }
        public string NewTier { get; set; } = "";
    }

    /// <summary>Thông tin PvP rating của user.</summary>
    public class PvpRatingDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public int PvpRating { get; set; }
        public int PvpWins { get; set; }
        public int PvpLosses { get; set; }
        public string Tier { get; set; } = "";
    }
}
