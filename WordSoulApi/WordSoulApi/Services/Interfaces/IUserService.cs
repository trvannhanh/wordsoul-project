

using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Services.Interfaces
{
    public interface IUserService
    {
        // Gán vai trò cho người dùng
        Task<bool> AssignRoleToUserAsync(int userId, string roleName);

        // Xóa người dùng theo ID
        Task<bool> DeleteUserAsync(int id);
        // Lấy tất cả người dùng
        Task<IEnumerable<UserDto>> GetAllUsersAsync(string? name, string? email, UserRole? role, int pageNumber, int pageSize);
        // Lấy bảng xếp hạng
        Task<List<LeaderBoardDto>> GetLeaderBoardAsync(bool? topXP, bool? topAP, int pageNumber, int pageSize);

        // Lấy người dùng theo ID
        Task<UserDetailDto?> GetUserByIdAsync(int userId);

        // Cập nhật người dùng
        Task<UserDto> UpdateUserAsync(int id, UserDto userDto);
    }
}