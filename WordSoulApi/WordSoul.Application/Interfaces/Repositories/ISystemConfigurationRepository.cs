using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface ISystemConfigurationRepository
    {
        Task<SystemConfiguration?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task<List<SystemConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);
        Task UpdateAsync(SystemConfiguration config, CancellationToken cancellationToken = default);
        Task UpdateBulkAsync(IEnumerable<SystemConfiguration> configs, CancellationToken cancellationToken = default);
    }
}
