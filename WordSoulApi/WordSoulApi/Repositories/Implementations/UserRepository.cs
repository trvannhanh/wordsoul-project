using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly WordSoulDbContext _context;
        public UserRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả người dùng
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        // Lấy người dùng theo ID
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }

        // Cập nhật thông tin người dùng
        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Xóa người dùng theo ID
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
