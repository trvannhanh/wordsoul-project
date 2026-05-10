using WordSoul.Application.DTOs.User;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện trừu tượng hóa việc giao tiếp với Google OAuth API.
    /// Implementation cụ thể nằm ở Infrastructure layer.
    /// </summary>
    public interface IGoogleOAuthService
    {
        /// <summary>
        /// Exchange Google Authorization Code → lấy thông tin người dùng Google.
        /// Trả về null nếu thất bại.
        /// </summary>
        Task<GoogleUserInfoDto?> ExchangeCodeForUserInfoAsync(string code, CancellationToken ct = default);
    }
}
