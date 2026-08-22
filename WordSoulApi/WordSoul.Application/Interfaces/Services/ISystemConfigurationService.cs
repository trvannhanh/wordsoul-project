using WordSoul.Application.DTOs.Admin;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Services
{
    public interface ISystemConfigurationService
    {
        Task<List<SystemConfiguration>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default);
        Task<SystemConfiguration?> GetConfigurationByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task UpdateConfigurationsAsync(
            IEnumerable<UpdateSystemConfigurationDto> configurations,
            string updatedBy,
            CancellationToken cancellationToken = default);
        Task<SystemConfiguration> CreateConfigurationAsync(SystemConfiguration config, CancellationToken cancellationToken = default);
        Task<SystemConfiguration> UpdateConfigurationAsync(string key, SystemConfiguration config, CancellationToken cancellationToken = default);
        Task<bool> DeleteConfigurationAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads a SystemConfiguration value by key and converts it to <typeparamref name="T"/>.
        /// Returns <paramref name="defaultValue"/> if the key is missing or cannot be parsed.
        /// </summary>
        Task<T> GetValueAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default)
            where T : IParsable<T>;
    }
}

