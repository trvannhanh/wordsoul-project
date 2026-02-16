
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserItemRepository
    {
        Task<UserItem?> GetUserItemAsync(int userId, int itemId, CancellationToken ct = default);
        Task<List<UserItem>> GetUserItemsAsync(int userId, CancellationToken ct = default);
        Task CreateUserItemAsync(UserItem userItem, CancellationToken ct = default);
        Task UpdateUserItemAsync(UserItem userItem, CancellationToken ct = default);
    }
}
