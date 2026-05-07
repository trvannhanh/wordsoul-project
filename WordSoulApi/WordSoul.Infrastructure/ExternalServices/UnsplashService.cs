using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.ExternalServices
{
    /// <summary>
    /// Lấy URL ảnh từ Unsplash API theo từ khóa.
    /// Port logic từ seed_vocabularies.py → C#.
    /// </summary>
    public class UnsplashService : IUnsplashService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UnsplashService> _logger;
        private readonly string _accessKey;

        private const string BaseUrl = "https://api.unsplash.com/search/photos";

        public UnsplashService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<UnsplashService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _accessKey = configuration["Unsplash:AccessKey"]
                ?? throw new InvalidOperationException("Unsplash:AccessKey is not configured.");
        }

        public async Task<string?> GetFirstImageUrlAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            try
            {
                var url = $"{BaseUrl}?query={Uri.EscapeDataString(query)}&per_page=1&orientation=landscape&client_id={_accessKey}";

                _logger.LogDebug("Fetching Unsplash image for query: '{Query}'", query);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var response = await _httpClient.GetAsync(url, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Unsplash API error {Status} for query '{Query}'", response.StatusCode, query);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var results = root.GetProperty("results");
                if (results.GetArrayLength() == 0)
                {
                    _logger.LogDebug("No Unsplash images found for '{Query}'", query);
                    return null;
                }

                // Lấy URL "regular" (~1080px) — stable Unsplash CDN URL
                var imageUrl = results[0]
                    .GetProperty("urls")
                    .GetProperty("regular")
                    .GetString();

                _logger.LogDebug("Unsplash image found for '{Query}': {Url}", query, imageUrl);
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnsplashService failed for query='{Query}'", query);
                return null;
            }
        }
    }
}
