using WordSoulApi.Models.DTOs.User;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserService
    {
        // Xóa người dùng theo ID
        Task<bool> DeleteUserAsync(int id);
        // Lấy tất cả người dùng
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        // Lấy người dùng theo ID
        Task<UserDto?> GetUserByIdAsync(int id);
        // Cập nhật người dùng
        Task<UserDto> UpdateUserAsync(int id, UserDto userDto);
    }
}