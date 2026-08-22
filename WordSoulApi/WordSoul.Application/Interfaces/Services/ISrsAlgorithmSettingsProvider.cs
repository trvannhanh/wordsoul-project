using WordSoul.Application.Services.SRS;

namespace WordSoul.Application.Interfaces.Services;

public interface ISrsAlgorithmSettingsProvider
{
    Task<SrsAlgorithmSettings> GetSettingsAsync(
        CancellationToken cancellationToken = default);
}
