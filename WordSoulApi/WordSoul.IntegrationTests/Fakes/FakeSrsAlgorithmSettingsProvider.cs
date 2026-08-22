using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services.SRS;

namespace WordSoul.IntegrationTests.Fakes;

public sealed class FakeSrsAlgorithmSettingsProvider(
    SrsAlgorithmSettings? settings = null)
    : ISrsAlgorithmSettingsProvider
{
    private readonly SrsAlgorithmSettings _settings =
        settings ?? SrsAlgorithmSettings.Default;

    public Task<SrsAlgorithmSettings> GetSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_settings);
    }
}
