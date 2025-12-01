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
        public async Task CreateItemAsync(Item item)
        {
            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
        }


        //-------------------------READ----------------------
        public async Task<List<Item>> GetItemAsync()
        {
            return await _context.Items.ToListAsync();
        }




    }
}
