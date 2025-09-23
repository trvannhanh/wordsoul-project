using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task CreateItemAsync(Item item);
        Task<List<Item>> GetItemAsync();
    }
}