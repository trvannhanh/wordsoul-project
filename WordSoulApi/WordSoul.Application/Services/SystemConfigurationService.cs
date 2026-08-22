using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.Services.SRS;
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

        public async Task UpdateConfigurationsAsync(
            IEnumerable<UpdateSystemConfigurationDto> configurations,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var updates = configurations.ToList();
                var duplicateKey = updates
                    .GroupBy(item => item.Key, StringComparer.Ordinal)
                    .FirstOrDefault(group => group.Count() > 1)
                    ?.Key;
                if (duplicateKey is not null)
                {
                    throw new ArgumentException(
                        $"Configuration key '{duplicateKey}' was submitted more than once.");
                }

                var existingConfigurations = await _unitOfWork
                    .SystemConfiguration
                    .GetAllAsync(cancellationToken);
                var existingByKey = existingConfigurations.ToDictionary(
                    item => item.Key,
                    StringComparer.Ordinal);
                var changedConfigurations = new List<SystemConfiguration>();

                foreach (var update in updates)
                {
                    if (!existingByKey.TryGetValue(update.Key, out var existing))
                    {
                        throw new KeyNotFoundException(
                            $"Configuration with key '{update.Key}' not found.");
                    }

                    ValidateConfiguration(
                        existing.DataType,
                        update.Value,
                        existing.MinValue,
                        existing.MaxValue);

                    if (AreEquivalent(
                        existing.DataType,
                        existing.Value,
                        update.Value))
                    {
                        continue;
                    }

                    if (!existing.IsLiveEditable)
                    {
                        throw new InvalidOperationException(
                            $"Configuration '{update.Key}' is not live-editable.");
                    }

                    existing.Value = update.Value;
                    existing.LastUpdatedBy = updatedBy;
                    changedConfigurations.Add(existing);
                }

                if (changedConfigurations.Count == 0)
                    return;

                if (changedConfigurations.Any(
                    item => SrsAlgorithmSettings.AlgorithmKeys.Contains(
                        item.Key)))
                {
                    if (!existingByKey.TryGetValue(
                        SrsAlgorithmSettings.PolicyVersionKey,
                        out var versionConfiguration)
                        || !int.TryParse(
                            versionConfiguration.Value,
                            System.Globalization.NumberStyles.Integer,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var currentVersion))
                    {
                        throw new InvalidOperationException(
                            "SRS policy version configuration is missing or invalid.");
                    }

                    versionConfiguration.Value =
                        checked(currentVersion + 1).ToString(
                            System.Globalization.CultureInfo.InvariantCulture);
                    versionConfiguration.LastUpdatedBy = updatedBy;
                    changedConfigurations.Add(versionConfiguration);

                    SrsAlgorithmSettings
                        .FromConfigurations(existingConfigurations);
                }

                await _unitOfWork.SystemConfiguration.UpdateBulkAsync(
                    changedConfigurations,
                    cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Updated {Count} system configurations in bulk by {UpdatedBy}.",
                    changedConfigurations.Count,
                    updatedBy);
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

            ValidateDefinition(config);
            ValidateConfiguration(
                config.DataType,
                config.Value,
                config.MinValue,
                config.MaxValue);

            config.LastUpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SystemConfiguration.AddAsync(config, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created system configuration key='{Key}'", config.Key);
            return config;
        }

        public async Task<SystemConfiguration> UpdateConfigurationAsync(string key, SystemConfiguration config, CancellationToken cancellationToken = default)
        {
            if (SrsAlgorithmSettings.AlgorithmKeys.Contains(key)
                || string.Equals(
                    key,
                    SrsAlgorithmSettings.PolicyVersionKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "SRS settings must be updated through the versioned bulk configuration endpoint.");
            }

            var existing = await _unitOfWork.SystemConfiguration.GetByKeyAsync(key, cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Configuration with key '{key}' not found.");
            }

            if (!existing.IsLiveEditable)
            {
                throw new InvalidOperationException(
                    $"Configuration '{key}' is not live-editable.");
            }

            ValidateConfiguration(
                existing.DataType,
                config.Value,
                existing.MinValue,
                existing.MaxValue);

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
            if (SrsAlgorithmSettings.AlgorithmKeys.Contains(key)
                || string.Equals(
                    key,
                    SrsAlgorithmSettings.PolicyVersionKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Versioned SRS settings cannot be deleted.");
            }

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

        private static void ValidateDefinition(SystemConfiguration config)
        {
            if (config.MinValue.HasValue
                && config.MaxValue.HasValue
                && config.MinValue > config.MaxValue)
            {
                throw new ArgumentException(
                    "Minimum configuration value cannot exceed maximum value.");
            }

            if ((config.MinValue.HasValue || config.MaxValue.HasValue)
                && !string.Equals(
                    config.DataType,
                    "Integer",
                    StringComparison.OrdinalIgnoreCase)
                && !string.Equals(
                    config.DataType,
                    "Float",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Only numeric configurations can define minimum or maximum values.");
            }
        }

        private static void ValidateConfiguration(
            string dataType,
            string value,
            double? minValue = null,
            double? maxValue = null)
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
                if (!int.TryParse(
                    value,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var parsed))
                {
                    throw new ArgumentException("Value must be a valid integer for Integer data type.");
                }

                ValidateNumericBounds(parsed, minValue, maxValue);
            }
            else if (string.Equals(dataType, "Float", StringComparison.OrdinalIgnoreCase))
            {
                if (!double.TryParse(
                    value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var parsed)
                    || !double.IsFinite(parsed))
                {
                    throw new ArgumentException("Value must be a valid float/double for Float data type.");
                }

                ValidateNumericBounds(parsed, minValue, maxValue);
            }
        }

        private static void ValidateNumericBounds(
            double value,
            double? minValue,
            double? maxValue)
        {
            if (minValue.HasValue && value < minValue.Value)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Value must be greater than or equal to {minValue.Value}.");
            }

            if (maxValue.HasValue && value > maxValue.Value)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Value must be less than or equal to {maxValue.Value}.");
            }
        }

        private static bool AreEquivalent(
            string dataType,
            string currentValue,
            string updatedValue)
        {
            if (string.Equals(
                dataType,
                "Integer",
                StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(
                        currentValue,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var currentInteger)
                    && int.TryParse(
                        updatedValue,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var updatedInteger)
                    && currentInteger == updatedInteger;
            }

            if (string.Equals(
                dataType,
                "Float",
                StringComparison.OrdinalIgnoreCase))
            {
                return double.TryParse(
                        currentValue,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var currentFloat)
                    && double.TryParse(
                        updatedValue,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var updatedFloat)
                    && currentFloat.Equals(updatedFloat);
            }

            if (string.Equals(
                dataType,
                "Boolean",
                StringComparison.OrdinalIgnoreCase))
            {
                return bool.TryParse(currentValue, out var currentBoolean)
                    && bool.TryParse(updatedValue, out var updatedBoolean)
                    && currentBoolean == updatedBoolean;
            }

            return string.Equals(
                currentValue,
                updatedValue,
                StringComparison.Ordinal);
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
