
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IUserInventoryService
    {
        Task AddItemToUserAsync(int userId, int itemId, int quantity, CancellationToken ct = default);
        Task RemoveItemFromUserAsync(int userId, int itemId, int quantity, CancellationToken ct = default);
        Task<List<UserItem>> GetUserInventoryAsync(int userId, CancellationToken ct = default);
    }
}
