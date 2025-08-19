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

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CheckUserVocabualryExist(int userId, int vocabId)
        {
            var exists = await _context.UserVocabularySets.AnyAsync(x => x.UserId == userId && x.VocabularySetId == vocabId);
            return exists;
        }

        public async Task AddVocabularySetToUserAsync(UserVocabularySet userVocabularySet)
        {
            _context.UserVocabularySets.Add(userVocabularySet);
            await _context.SaveChangesAsync();
        }
    }
}
