using WordSoulApi.Models.DTOs.Home;
using WordSoulApi.Models.DTOs.User;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> AssignRoleToUserAsync(int userId, string roleName);

        // Xóa người dùng theo ID
        Task<bool> DeleteUserAsync(int id);
        // Lấy tất cả người dùng
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        // Lấy người dùng theo ID
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDashboardDto> GetUserDashboardAsync(int userId);

        // Cập nhật người dùng
        Task<UserDto> UpdateUserAsync(int id, UserDto userDto);
    }
}