using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Xóa người dùng theo ID
        Task<bool> DeleteUserAsync(int id);
        // Lấy tất cả người dùng
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<List<DateTime>> GetLearningSessionDatesAsync(int userId);

        // Lấy người dùng theo ID
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserWithRelationsAsync(int userId);

        // Cập nhật thông tin người dùng
        Task<User> UpdateUserAsync(User user);
        Task<(int XP, int AP)> UpdateUserXPAndAPAsync(int userId, int xp, int ap);
    }
}