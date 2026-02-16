
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class UserInventoryService : IUserInventoryService
    {
        private readonly IUnitOfWork _uow;

        private static readonly Dictionary<int, SemaphoreSlim> _locks = new();

        public UserInventoryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        private SemaphoreSlim GetLock(int userId)
        {
            lock (_locks)
            {
                if (!_locks.ContainsKey(userId))
                    _locks[userId] = new SemaphoreSlim(1, 1);

                return _locks[userId];
            }
        }

        public async Task AddItemToUserAsync(int userId, int itemId, int quantity, CancellationToken ct = default)
        {
            var userLock = GetLock(userId);
            await userLock.WaitAsync(ct);

            try
            {
                var existing = await _uow.UserItem
                    .GetUserItemAsync(userId, itemId, ct);

                if (existing == null)
                {
                    await _uow.UserItem.CreateUserItemAsync(new UserItem
                    {
                        UserId = userId,
                        ItemId = itemId,
                        Quantity = quantity
                    }, ct);
                }
                else
                {
                    existing.Quantity += quantity;
                    await _uow.UserItem.UpdateUserItemAsync(existing, ct);
                }

                await _uow.SaveChangesAsync(ct);
            }
            finally
            {
                userLock.Release();
            }
        }

        public async Task RemoveItemFromUserAsync(int userId, int itemId, int quantity, CancellationToken ct = default)
        {
            var userLock = GetLock(userId);
            await userLock.WaitAsync(ct);

            try
            {
                var existing = await _uow.UserItem
                    .GetUserItemAsync(userId, itemId, ct);

                if (existing == null || existing.Quantity < quantity)
                    throw new InvalidOperationException("Not enough items.");

                existing.Quantity -= quantity;

                await _uow.UserItem.UpdateUserItemAsync(existing, ct);
                await _uow.SaveChangesAsync(ct);
            }
            finally
            {
                userLock.Release();
            }
        }

        public async Task<List<UserItem>> GetUserInventoryAsync(int userId, CancellationToken ct = default)
        {
            return await _uow.UserItem.GetUserItemsAsync(userId, ct);
        }
    }
}
