using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Services
{
    public interface ISystemConfigurationService
    {
        Task<List<SystemConfiguration>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default);
        Task<SystemConfiguration?> GetConfigurationByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task UpdateConfigurationsAsync(IEnumerable<SystemConfiguration> configurations, CancellationToken cancellationToken = default);
    }
}
