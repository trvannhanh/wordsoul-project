using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WordSoul.Application.DTOs.VocabularySet;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.ExternalServices
{
    /// <summary>
    /// Triển khai cache AI vocabulary preview sử dụng Redis (IDistributedCache).
    /// Cache per-word với TTL 7 ngày. Fallback gracefully nếu Redis không khả dụng.
    /// </summary>
    public class VocabularyAiCacheService : IVocabularyAiCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<VocabularyAiCacheService> _logger;
        private static readonly TimeSpan Ttl = TimeSpan.FromDays(7);
        private const string KeyPrefix = "vocab-ai-preview:";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public VocabularyAiCacheService(IDistributedCache cache, ILogger<VocabularyAiCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<VocabularyPreviewDto?> GetAsync(string word, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(word);
            try
            {
                var json = await _cache.GetStringAsync(key, cancellationToken);
                if (json is null) return null;

                var dto = JsonSerializer.Deserialize<VocabularyPreviewDto>(json, JsonOptions);
                _logger.LogDebug("Cache HIT for vocab-ai-preview '{Word}'", word);
                return dto;
            }
            catch (Exception ex)
            {
                // Redis không khả dụng → fallback bình thường, không crash
                _logger.LogWarning(ex, "Redis GET failed for key '{Key}' — falling back to Gemini", key);
                return null;
            }
        }

        public async Task SetAsync(string word, VocabularyPreviewDto dto, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(word);
            try
            {
                var json = JsonSerializer.Serialize(dto, JsonOptions);
                await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = Ttl
                }, cancellationToken);

                _logger.LogDebug("Cache SET for vocab-ai-preview '{Word}' (TTL={Ttl})", word, Ttl);
            }
            catch (Exception ex)
            {
                // Lỗi write cache không nên làm hỏng luồng chính
                _logger.LogWarning(ex, "Redis SET failed for key '{Key}' — data will not be cached", key);
            }
        }

        private static string BuildKey(string word) =>
            $"{KeyPrefix}{word.Trim().ToLowerInvariant()}";
    }
}
