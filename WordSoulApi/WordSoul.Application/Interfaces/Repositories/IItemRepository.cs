using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IItemRepository
    {
        Task CreateItemAsync(Item item);
        Task<List<Item>> GetItemAsync();
    }
}