using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.User;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ xác thực người dùng.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Đăng nhập bằng username và password.
        /// </summary>
        Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken ct = default);

        /// <summary>
        /// Đăng ký tài khoản mới.
        /// </summary>
        Task<UserDto?> RegisterAsync(RegisterDto registerDto, CancellationToken ct = default);

        /// <summary>
        /// Làm mới AccessToken bằng RefreshToken.
        /// </summary>
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default);
    }
}
