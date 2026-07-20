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
                // Validate bulk items before saving
                foreach (var config in configurations)
                {
                    ValidateConfiguration(config.DataType, config.Value);
                }

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

        public async Task<SystemConfiguration> CreateConfigurationAsync(SystemConfiguration config, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.SystemConfiguration.GetByKeyAsync(config.Key, cancellationToken);
            if (existing != null)
            {
                throw new ArgumentException($"Configuration with key '{config.Key}' already exists.");
            }

            ValidateConfiguration(config.DataType, config.Value);

            config.LastUpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SystemConfiguration.AddAsync(config, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created system configuration key='{Key}'", config.Key);
            return config;
        }

        public async Task<SystemConfiguration> UpdateConfigurationAsync(string key, SystemConfiguration config, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.SystemConfiguration.GetByKeyAsync(key, cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Configuration with key '{key}' not found.");
            }

            ValidateConfiguration(existing.DataType, config.Value);

            existing.Value = config.Value;
            if (config.Description != null) existing.Description = config.Description;
            if (config.Category != null) existing.Category = config.Category;
            existing.LastUpdatedBy = config.LastUpdatedBy;
            existing.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SystemConfiguration.UpdateAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated system configuration key='{Key}'", key);
            return existing;
        }

        public async Task<bool> DeleteConfigurationAsync(string key, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.SystemConfiguration.GetByKeyAsync(key, cancellationToken);
            if (existing == null)
            {
                return false;
            }

            await _unitOfWork.SystemConfiguration.DeleteAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted system configuration key='{Key}'", key);
            return true;
        }

        private void ValidateConfiguration(string dataType, string value)
        {
            if (string.Equals(dataType, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Value must be 'true' or 'false' for Boolean data type.");
                }
            }
            else if (string.Equals(dataType, "Integer", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(value, out _))
                {
                    throw new ArgumentException("Value must be a valid integer for Integer data type.");
                }
            }
            else if (string.Equals(dataType, "Float", StringComparison.OrdinalIgnoreCase))
            {
                if (!double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    throw new ArgumentException("Value must be a valid float/double for Float data type.");
                }
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
