using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginDto userDto);
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<User> RegisterAsync(RegisterDto userDto);
    }
}