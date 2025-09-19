using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        //-------------------------------- CREATE -----------------------------------


        //------------------------------- READ -----------------------------------
        // Lấy tất cả người dùng
        Task<IEnumerable<User>> GetAllUsersAsync(string? name, string? email, UserRole? role, bool? topXP, bool? topAP, int pageNumber, int pageSize);
        // Lấy ngày học tập của người dùng
        Task<List<DateTime>> GetLearningSessionDatesAsync(int userId);

        // Lấy người dùng theo ID
        Task<User?> GetUserByIdAsync(int id);
        // Lấy người dùng với thông tin liên quan 
        Task<User?> GetUserWithRelationsAsync(int userId);

        //------------------------------- UPDATE -----------------------------------

        // Cập nhật thông tin người dùng
        Task<User> UpdateUserAsync(User user);
        // Cập nhật XP và AP cho người dùng
        Task<(int XP, int AP)> UpdateUserXPAndAPAsync(int userId, int xp, int ap);

        //-------------------------------- DELETE -----------------------------------
        // Xóa người dùng theo ID
        Task<bool> DeleteUserAsync(int id);
        

    }
}