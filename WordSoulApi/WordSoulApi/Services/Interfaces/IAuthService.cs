using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IAuthService
    {
        // Đăng nhập người dùng và trả về TokenResponseDto nếu thành công, ngược lại trả về null
        Task<TokenResponseDto?> LoginAsync(LoginDto userDto);
        // Làm mới token và trả về TokenResponseDto nếu thành công, ngược lại trả về null
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        // Đăng ký người dùng mới và trả về User nếu thành công, ngược lại trả về null
        Task<User> RegisterAsync(RegisterDto userDto);
    }
}