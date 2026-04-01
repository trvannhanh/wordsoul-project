using WordSoul.Application.DTOs.Battle;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service cho Real-time Arena Battle (PvE Gym Leader bot + PvP).
    /// Xử lý toàn bộ state machine của một battle session.
    /// </summary>
    public interface IArenaBattleService
    {
        // ── PvE (Gym Battle) ───────────────────────────────────────────────────

        /// <summary>Tạo mới BattleSession từ REST API – kiểm tra điều kiện, lưu DB, trả về sessionId.</summary>
        Task<int> CreateSessionAsync(
            StartArenaBattleRequestDto dto,
            int userId,
            CancellationToken ct = default);

        /// <summary>Khi player vào Hub và gọi PlayerReady: khởi tạo PetStates, lấy câu hỏi first round.</summary>
        Task<BattleStartedDto?> StartBattleAsync(int sessionId, int userId, CancellationToken ct = default);

        /// <summary>Nhận câu trả lời từ người chơi (PvE). Bot giả lập ngay lập tức.</summary>
        Task<SubmitAnswerResultWrapper?> SubmitAnswerAsync(
            SubmitRoundAnswerDto dto,
            int userId,
            CancellationToken ct = default);

        // ── PvP ───────────────────────────────────────────────────────────────

        /// <summary>Người chơi A tạo phòng PvP, nhận RoomCode 6 ký tự.</summary>
        Task<PvpRoomCreatedDto> CreatePvpSessionAsync(
            CreatePvpSessionDto dto,
            int userId,
            CancellationToken ct = default);

        /// <summary>Người chơi B join phòng bằng RoomCode. Trả về sessionId và opponentUserId (cho notification).</summary>
        Task<(int sessionId, int opponentUserId)> JoinPvpSessionAsync(
            JoinPvpSessionDto dto,
            int userId,
            CancellationToken ct = default);

        /// <summary>Cả 2 player đã vào Hub. Khởi tạo PetStates, lưu ConnectionId, trả về BattleStartedDto.</summary>
        Task<BattleStartedDto?> StartPvpBattleAsync(
            int sessionId,
            int userId,
            string connectionId,
            CancellationToken ct = default);

        /// <summary>
        /// Nhận câu trả lời PvP. Buffer câu trả lời vào cache, khi đủ 2 bên mới resolve round.
        /// Trả về null nếu chưa đủ 2 bên (client sẽ nhận WaitingOpponent qua Hub).
        /// </summary>
        Task<SubmitAnswerResultWrapper?> SubmitPvpAnswerAsync(
            SubmitRoundAnswerDto dto,
            int userId,
            CancellationToken ct = default);

        /// <summary>Xử lý khi người chơi disconnect – bên forfeit thua ngay, ELO bị trừ.</summary>
        Task<BattleEndedDto?> ForfeitPvpBattleAsync(
            string connectionId,
            CancellationToken ct = default);

        /// <summary>Lấy PvP rating của user.</summary>
        Task<PvpRatingDto?> GetPvpRatingAsync(int userId, CancellationToken ct = default);
    }
}
