using WordSoul.Application.DTOs.Gym;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IGymLeaderService
    {
        /// <summary>
        /// Trả về danh sách tất cả 8 Gym Leader kèm trạng thái tiến trình của user.
        /// </summary>
        Task<List<GymLeaderDto>> GetAllGymsForUserAsync(int userId, CancellationToken ct = default);

        /// <summary>
        /// Trả về chi tiết một Gym Leader cụ thể, kèm tiến trình và cooldown.
        /// </summary>
        Task<GymLeaderDto?> GetGymDetailAsync(int userId, int gymId, CancellationToken ct = default);

        /// <summary>
        /// Kiểm tra và tự động unlock các Gym mà user đã đủ điều kiện.
        /// Gọi sau mỗi LearningSession hoàn thành.
        /// </summary>
        Task CheckAndUnlockGymsAsync(int userId, CancellationToken ct = default);
    }
}
