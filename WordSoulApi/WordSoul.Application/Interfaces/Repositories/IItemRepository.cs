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

        Task<Item?> GetItemByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        Task UpdateItemAsync(
            Item item,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE -----------------------------
        Task DeleteItemAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}