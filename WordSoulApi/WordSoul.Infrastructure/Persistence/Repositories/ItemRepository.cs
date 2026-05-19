using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly WordSoulDbContext _context;

        public ItemRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //------------------------CREATE---------------------
        public async Task CreateItemAsync(Item item, CancellationToken cancellationToken = default)
        {
            await _context.Items.AddAsync(item, cancellationToken);
        }

        //-------------------------READ----------------------
        public async Task<List<Item>> GetItemAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Items.OrderBy(i => i.Type).ThenBy(i => i.Name).ToListAsync(cancellationToken);
        }

        public async Task<Item?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Items.FindAsync([id], cancellationToken);
        }

        //------------------------UPDATE---------------------
        public Task UpdateItemAsync(Item item, CancellationToken cancellationToken = default)
        {
            _context.Items.Update(item);
            return Task.CompletedTask;
        }

        //------------------------DELETE---------------------
        public async Task DeleteItemAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await _context.Items.FindAsync([id], cancellationToken);
            if (item != null)
                _context.Items.Remove(item);
        }
    }
}