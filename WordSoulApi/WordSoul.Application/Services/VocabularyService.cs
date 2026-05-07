using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class VocabularyService : IVocabularyService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache _cache;
        private readonly ILogger<VocabularyService> _logger;

        // Thread-safe theo dõi key để có thể xóa cache theo prefix khi cần
        private readonly HashSet<string> _cacheKeys = new();
        private readonly object _cacheLock = new();

        public VocabularyService(
            IUnitOfWork uow,
            ILogger<VocabularyService> logger,
            IMemoryCache cache)
        {
            _uow = uow;
            _logger = logger;
            _cache = cache;
        }

        // ============================================================================
        // CREATE
        // ============================================================================

        /// <summary>
        /// Tạo từ vựng mới (dùng cho admin).
        /// </summary>
        public async Task<AdminVocabularyDto> CreateVocabularyAsync(
            CreateVocabularyDto dto,
            string? imageUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Word))
                throw new ArgumentException("Word is required.", nameof(dto.Word));

            _logger.LogInformation("Creating new vocabulary: {Word}", dto.Word);

            var vocabulary = new Vocabulary
            {
                Word = dto.Word.Trim(),
                Meaning = dto.Meaning?.Trim(),
                Pronunciation = dto.Pronunciation?.Trim(),
                PartOfSpeech = dto.PartOfSpeech,
                CEFRLevel = dto.CEFRLevel,
                Description = dto.Description?.Trim(),
                ExampleSentence = dto.ExampleSentence?.Trim(),
                ImageUrl = imageUrl,
                PronunciationUrl = dto.PronunciationUrl?.Trim()
            };

            await _uow.Vocabulary.CreateVocabularyAsync(vocabulary, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vocabulary created with ID {Id}", vocabulary.Id);

            return MapToAdminDto(vocabulary);
        }

        // ============================================================================
        // READ
        // ============================================================================

        /// <summary>
        /// Lấy danh sách từ vựng với bộ lọc và phân trang (có cache 15 phút).
        /// </summary>
        public async Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync(
            string? word = null,
            string? meaning = null,
            PartOfSpeech? partOfSpeech = null,
            CEFRLevel? cefrLevel = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"VocabList_{word}_{meaning}_{partOfSpeech}_{cefrLevel}_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<VocabularyDto> cached))
            {
                _logger.LogDebug("Cache HIT – {CacheKey}", cacheKey);
                return cached;
            }

            var entities = await _uow.Vocabulary.GetAllVocabulariesAsync(
                word, meaning, partOfSpeech, cefrLevel, pageNumber, pageSize, cancellationToken);

            var dtos = entities.Select(MapToDto).ToList();

            var entryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .SetSize(1); // đơn vị tùy dự án, chỉ để IMemoryCache biết kích thước

            lock (_cacheLock)
            {
                _cache.Set(cacheKey, dtos, entryOptions);
                _cacheKeys.Add(cacheKey);
            }

            _logger.LogDebug("Cache MISS & stored – {CacheKey}", cacheKey);
            return dtos;
        }

        /// <summary>
        /// Lấy chi tiết một từ vựng theo ID (có cache 30 phút).
        /// </summary>
        public async Task<VocabularyDto?> GetVocabularyByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"Vocab_{id}";

            if (_cache.TryGetValue(cacheKey, out VocabularyDto cached))
            {
                _logger.LogDebug("Cache HIT – {CacheKey}", cacheKey);
                return cached;
            }

            var entity = await _uow.Vocabulary.GetVocabularyByIdAsync(id, cancellationToken);
            if (entity == null)
                return null;

            var dto = MapToDto(entity);

            var entryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .SetSize(1);

            lock (_cacheLock)
            {
                _cache.Set(cacheKey, dto, entryOptions);
                _cacheKeys.Add(cacheKey);
            }

            _logger.LogDebug("Cache MISS & stored – {CacheKey}", cacheKey);
            return dto;
        }

        /// <summary>
        /// Lấy nhiều từ vựng theo danh sách từ (word list) – thường dùng khi tạo session học.
        /// </summary>
        public async Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(
            SearchVocabularyDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto?.Words == null || !dto.Words.Any())
                return Enumerable.Empty<VocabularyDto>();

            var orderedWords = dto.Words.OrderBy(w => w).ToList();
            var cacheKey = $"VocabByWords_{string.Join("_", orderedWords)}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<VocabularyDto> cached))
            {
                _logger.LogDebug("Cache HIT – {CacheKey}", cacheKey);
                return cached;
            }

            var entities = await _uow.Vocabulary.GetVocabulariesByWordsAsync(orderedWords, null, cancellationToken);

            var dtos = entities.Select(MapToDto).ToList();

            var entryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSize(1);

            lock (_cacheLock)
            {
                _cache.Set(cacheKey, dtos, entryOptions);
                _cacheKeys.Add(cacheKey);
            }

            _logger.LogDebug("Cache MISS & stored – {CacheKey}", cacheKey);
            return dtos;
        }

        // ============================================================================
        // UPDATE
        // ============================================================================

        /// <summary>
        /// Cập nhật từ vựng (admin only).
        /// </summary>
        public async Task<AdminVocabularyDto?> UpdateVocabularyAsync(
            int id,
            CreateVocabularyDto dto,
            string? imageUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Word))
                throw new ArgumentException("Word is required.", nameof(dto.Word));

            _logger.LogInformation("Updating vocabulary ID {Id}", id);

            var entity = await _uow.Vocabulary.GetVocabularyByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Vocabulary with ID {id} not found.");

            // Cập nhật các field
            entity.Word = dto.Word.Trim();
            entity.Meaning = dto.Meaning?.Trim();
            entity.Pronunciation = dto.Pronunciation?.Trim();
            entity.PartOfSpeech = dto.PartOfSpeech;
            entity.CEFRLevel = dto.CEFRLevel;
            entity.Description = dto.Description?.Trim();
            entity.ExampleSentence = dto.ExampleSentence?.Trim();
            entity.ImageUrl = imageUrl ?? entity.ImageUrl; // giữ lại ảnh cũ nếu không upload mới
            entity.PronunciationUrl = dto.PronunciationUrl?.Trim();

            await _uow.Vocabulary.UpdateVocabularyAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // Xóa cache liên quan
            InvalidateCache($"Vocab_{id}");
            InvalidateCachePrefix("VocabList_");
            InvalidateCachePrefix("VocabByWords_");

            _logger.LogInformation("Vocabulary ID {Id} updated and related caches invalidated", id);

            return MapToAdminDto(entity);
        }

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Xóa từ vựng (admin only).
        /// </summary>
        public async Task<bool> DeleteVocabularyAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting vocabulary ID {Id}", id);

            var result = await _uow.Vocabulary.DeleteVocabularyAsync(id, cancellationToken);
            if (result)
            {
                await _uow.SaveChangesAsync(cancellationToken);

                InvalidateCache($"Vocab_{id}");
                InvalidateCachePrefix("VocabList_");
                InvalidateCachePrefix("VocabByWords_");

                _logger.LogInformation("Vocabulary ID {Id} deleted and caches invalidated", id);
            }

            return result;
        }

        // ============================================================================
        // PRIVATE HELPERS
        // ============================================================================

        private static VocabularyDto MapToDto(Vocabulary v) => new()
        {
            Id = v.Id,
            Word = v.Word,
            Meaning = v.Meaning,
            PartOfSpeech = v.PartOfSpeech.ToString(),
            CEFRLevel = v.CEFRLevel.ToString(),
            Description = v.Description,
            ExampleSentence = v.ExampleSentence,
            ImageUrl = v.ImageUrl,
            Pronunciation = v.Pronunciation,
            PronunciationUrl = v.PronunciationUrl
        };

        private static AdminVocabularyDto MapToAdminDto(Vocabulary v) => new()
        {
            Id = v.Id,
            Word = v.Word,
            Meaning = v.Meaning,
            Pronunciation = v.Pronunciation,
            PartOfSpeech = v.PartOfSpeech.ToString(),
            CEFRLevel = v.CEFRLevel.ToString(),
            Description = v.Description,
            ExampleSentence = v.ExampleSentence,
            ImageUrl = v.ImageUrl,
            PronunciationUrl = v.PronunciationUrl
        };

        private void InvalidateCache(string key)
        {
            lock (_cacheLock)
            {
                _cache.Remove(key);
                _cacheKeys.Remove(key);
            }
        }

        private void InvalidateCachePrefix(string prefix)
        {
            lock (_cacheLock)
            {
                var keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _cacheKeys.Remove(key);
                }
            }
        }
    }
}