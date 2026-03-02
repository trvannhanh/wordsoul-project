using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IItemRepository
    {
        // ----------------------------- CREATE -----------------------------
        Task CreateItemAsync(
            Item item,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        Task<List<Item>> GetItemAsync(
            CancellationToken cancellationToken = default);
    }
}