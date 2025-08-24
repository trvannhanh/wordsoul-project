using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Xóa người dùng theo ID
        Task<bool> DeleteUserAsync(int id);
        // Lấy tất cả người dùng
        Task<IEnumerable<User>> GetAllUsersAsync();
        // Lấy người dùng theo ID
        Task<User?> GetUserByIdAsync(int id);
        // Cập nhật thông tin người dùng
        Task<User> UpdateUserAsync(User user);
    }
}