using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace WordSoul.Application.Services
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SystemConfigurationService> _logger;

        public SystemConfigurationService(IUnitOfWork unitOfWork, ILogger<SystemConfigurationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<SystemConfiguration>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.SystemConfiguration.GetAllAsync(cancellationToken);
        }

        public async Task<SystemConfiguration?> GetConfigurationByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.SystemConfiguration.GetByKeyAsync(key, cancellationToken);
        }

        public async Task UpdateConfigurationsAsync(IEnumerable<SystemConfiguration> configurations, CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.SystemConfiguration.UpdateBulkAsync(configurations, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully updated system configurations in bulk.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system configurations.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetValueAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default)
            where T : IParsable<T>
        {
            try
            {
                var config = await _unitOfWork.SystemConfiguration.GetByKeyAsync(key, cancellationToken);
                if (config is null || string.IsNullOrWhiteSpace(config.Value))
                {
                    _logger.LogWarning("SystemConfig key '{Key}' not found, using default: {Default}", key, defaultValue);
                    return defaultValue;
                }

                if (T.TryParse(config.Value, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                    return parsed;

                _logger.LogWarning("SystemConfig key '{Key}' value '{Value}' could not be parsed as {Type}, using default: {Default}",
                    key, config.Value, typeof(T).Name, defaultValue);
                return defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading SystemConfig key '{Key}', using default: {Default}", key, defaultValue);
                return defaultValue;
            }
        }
    }
}
