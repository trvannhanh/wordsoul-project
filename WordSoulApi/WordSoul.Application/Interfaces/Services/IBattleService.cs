using WordSoul.Application.DTOs.Gym;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IBattleService
    {
        /// <summary>
        /// Bắt đầu một Gym Battle: kiểm tra cooldown, tạo BattleSession, chọn câu hỏi.
        /// Throws InvalidOperationException nếu Gym chưa Unlocked hoặc đang cooldown.
        /// </summary>
        Task<StartBattleResponseDto> StartGymBattleAsync(int userId, int gymId, CancellationToken ct = default);

        /// <summary>
        /// Submit kết quả battle: chấm điểm, cập nhật UserGymProgress, phát XP + Badge nếu thắng.
        /// </summary>
        Task<BattleResultDto> SubmitBattleAsync(int userId, int battleSessionId, SubmitBattleRequestDto request, CancellationToken ct = default);
    }
}
