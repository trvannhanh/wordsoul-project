using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Xml.XPath;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WordSoulApi.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly WordSoulDbContext _context;
        public UserRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //------------------------------- READ -----------------------------------

        // Lấy tất cả người dùng
        public async Task<IEnumerable<User>> GetAllUsersAsync(
            string? name,
            string? email,
            UserRole? role,         
            bool? topXP,
            bool? topAP,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Users
                .Include(u => u.UserOwnedPets)
                .AsQueryable();

            // Bộ lọc tên & email
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u => u.Username.Contains(name));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => u.Email.Contains(email));

            // Bộ lọc theo Role
            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            // Sắp xếp theo XP
            if (topXP.HasValue)
            {
                query = topXP.Value
                    ? query.OrderByDescending(u => u.XP)
                    : query.OrderBy(u => u.XP);
            }

            // Sắp xếp theo AP
            if (topAP.HasValue)
            {
                query = query.Expression.Type == typeof(IOrderedQueryable<User>)
                    ? (topAP.Value
                        ? ((IOrderedQueryable<User>)query).ThenByDescending(u => u.AP)
                        : ((IOrderedQueryable<User>)query).ThenBy(u => u.AP))
                    : (topAP.Value
                        ? query.OrderByDescending(u => u.AP)
                        : query.OrderBy(u => u.AP));
            }

            // Phân trang
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Lấy người dùng theo ID
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }

        // Lấy người dùng theo ID kèm các quan hệ
        public async Task<User?> GetUserWithRelationsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserOwnedPets).ThenInclude(up => up.Pet)
                .Include(u => u.UserVocabularyProgresses)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        // Lấy danh sách ngày có phiên học của người dùng
        public async Task<List<DateTime>> GetLearningSessionDatesAsync(int userId)
        {
            return await _context.LearningSessions
                .Where(s => s.UserId == userId)
                .Select(s => s.EndTime.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();
        }

        //------------------------------- UPDATE -----------------------------------

        // Cập nhật thông tin người dùng
        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
        // Cập nhật XP và AP của người dùng
        public async Task<(int XP, int AP)> UpdateUserXPAndAPAsync(int userId, int xp, int ap)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");
            user.XP += xp;
            user.AP += ap;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return (user.XP, user.AP);
        }

        //------------------------------- DELETE -----------------------------------
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
