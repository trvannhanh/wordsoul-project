using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
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
