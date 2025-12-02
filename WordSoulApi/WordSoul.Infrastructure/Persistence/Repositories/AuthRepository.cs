using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly WordSoulDbContext _context;

        public AuthRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //------------------------------- CREATE -----------------------------------
        public async Task<User> RegisterUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            return user;
        }

        //------------------------------- READ -----------------------------------
        public async Task<User?> LoginUserAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FindAsync([id], cancellationToken);
        }

        //------------------------------- UPDATE -----------------------------------
        public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            return await Task.FromResult(user);
        }

        //------------------------------- OTHER -----------------------------------
        public async Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }
    }
}