using WordSoul.Application.DTOs.User;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IAuthService
    {
        // Đăng nhập người dùng và trả về TokenResponseDto nếu thành công, ngược lại trả về null
        Task<TokenResponseDto?> LoginAsync(LoginDto userDto);
        // Làm mới token và trả về TokenResponseDto nếu thành công, ngược lại trả về null
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        // Đăng ký người dùng mới và trả về User nếu thành công, ngược lại trả về null
        Task<UserDto> RegisterAsync(RegisterDto userDto);
    }
}