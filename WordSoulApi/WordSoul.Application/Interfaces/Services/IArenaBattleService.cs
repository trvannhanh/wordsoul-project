using WordSoul.Application.DTOs.Battle;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service cho Real-time Arena Battle (PvE Gym Leader bot + tương lai PvP).
    /// Xử lý toàn bộ state machine của một battle session.
    /// </summary>
    public interface IArenaBattleService
    {
        /// <summary>
        /// Tạo mới BattleSession từ REST API – kiểm tra điều kiện, lưu DB, trả về sessionId.
        /// Yêu cầu người chơi có ≥ 3 UserOwnedPet.
        /// </summary>
        Task<int> CreateSessionAsync(
            StartArenaBattleRequestDto dto,
            int userId,
            CancellationToken ct = default);

        /// <summary>
        /// Khi player vào Hub và gọi PlayerReady: khởi tạo PetStates, lấy câu hỏi first round.
        /// Trả về null nếu session không hợp lệ.
        /// </summary>
        Task<BattleStartedDto?> StartBattleAsync(int sessionId, int userId, CancellationToken ct = default);

        /// <summary>
        /// Nhận câu trả lời từ người chơi.
        /// Nếu PvE: ngay lập tức sinh câu trả lời bot → tính kết quả → cập nhật HP.
        /// Trả về null nếu session không hợp lệ / đã kết thúc.
        /// </summary>
        Task<SubmitAnswerResultWrapper?> SubmitAnswerAsync(
            SubmitRoundAnswerDto dto,
            int userId,
            CancellationToken ct = default);
    }
}
