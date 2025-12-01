using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        // kiểm tra email đã tồn tại chưa
        Task<bool> EmailExistsAsync(string email);
        // Lấy người dùng theo ID
        Task<User?> GetUserByIdAsync(int id);
        // Đăng nhập người dùng
        Task<User?> LoginUserAsync(string username);
        //Đăng ký người dùng
        Task<User> RegisterUserAsync(User user);
        // Cập nhật thông tin người dùng
        Task<User> UpdateUserAsync(User user);
        // Kiểm tra tên người dùng đã tồn tại chưa
        Task<bool> UserExistsAsync(string username);
    }
}