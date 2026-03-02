using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class UserItemRepository : IUserItemRepository
    {
        private readonly WordSoulDbContext _context;

        public UserItemRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task<UserItem?> GetUserItemAsync(int userId, int itemId, CancellationToken ct = default)
        {
            return await _context.UserItems
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ItemId == itemId, ct);
        }

        public async Task<List<UserItem>> GetUserItemsAsync(int userId, CancellationToken ct = default)
        {
            return await _context.UserItems
                .Include(x => x.Item)
                .Where(x => x.UserId == userId)
                .ToListAsync(ct);
        }

        public async Task CreateUserItemAsync(UserItem userItem, CancellationToken ct = default)
        {
            await _context.UserItems.AddAsync(userItem, ct);
        }

        public Task UpdateUserItemAsync(UserItem userItem, CancellationToken ct = default)
        {
            _context.UserItems.Update(userItem);
            return Task.CompletedTask;
        }
    }
}
