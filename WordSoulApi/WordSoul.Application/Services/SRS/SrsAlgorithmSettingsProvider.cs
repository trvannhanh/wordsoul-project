using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Application.Services.SRS;

public sealed class SrsAlgorithmSettingsProvider(
    ISystemConfigurationService systemConfigurationService,
    ILogger<SrsAlgorithmSettingsProvider> logger)
    : ISrsAlgorithmSettingsProvider
{
    public async Task<SrsAlgorithmSettings> GetSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configurations = await systemConfigurationService
                .GetAllConfigurationsAsync(cancellationToken);
            return SrsAlgorithmSettings.FromConfigurations(configurations);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Invalid SRS configuration detected; using safe defaults.");
            return SrsAlgorithmSettings.Default;
        }
    }
}
