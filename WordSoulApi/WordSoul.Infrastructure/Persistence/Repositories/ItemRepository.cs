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
            return await _context.Items.ToListAsync(cancellationToken);
        }
    }
}