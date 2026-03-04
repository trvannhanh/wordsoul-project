
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.VocabularySet;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class VocabularySetService : IVocabularySetService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<VocabularySetService> _logger;
        private readonly IMemoryCache _cache;

        // Thread-safe cache key tracking
        private readonly HashSet<string> _cacheKeys = [];
        private readonly object _cacheLock = new();

        public VocabularySetService(
            IUnitOfWork uow,
            ILogger<VocabularySetService> logger,
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
        /// Tạo bộ từ vựng mới (admin hoặc người dùng tạo set riêng).
        /// Tự động gán reward pet theo rarity và thêm set vào thư viện của người tạo.
        /// </summary>
        public async Task<VocabularySetDto> CreateVocabularySetAsync(
            CreateVocabularySetDto dto,
            string? imageUrl,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.", nameof(dto.Title));

            _logger.LogInformation("User {UserId} is creating vocabulary set: {Title}", userId, dto.Title);

            var user = await _uow.User.GetUserByIdAsync(userId, cancellationToken)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            var uniqueVocabIds = dto.VocabularyIds.Distinct().ToList();
            if (uniqueVocabIds.Count != dto.VocabularyIds.Count)
                throw new ArgumentException("Vocabulary IDs must be unique.");

            if (uniqueVocabIds.Count > 50)
                throw new ArgumentException("Maximum 50 vocabularies per set.");

            // Validate all vocabularies exist
            foreach (var vocabId in uniqueVocabIds)
            {
                var exists = await _uow.Vocabulary.GetVocabularyByIdAsync(vocabId, cancellationToken) ?? throw new KeyNotFoundException($"Vocabulary ID {vocabId} not found.");
            }

            var vocabularySet = new VocabularySet
            {
                Title = dto.Title.Trim(),
                Theme = dto.Theme,
                ImageUrl = imageUrl,
                Description = dto.Description?.Trim(),
                DifficultyLevel = dto.DifficultyLevel,
                IsActive = dto.IsActive,
                IsPublic = dto.IsPublic,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                SetVocabularies = uniqueVocabIds
                    .Select((id, idx) => new SetVocabulary { VocabularyId = id, Order = idx + 1 })
                    .ToList()
            };

            // Auto-assign reward pets by rarity, ưu tiên type khớp với Theme
            var themePetTypes = ThemeToPetTypesMap.GetValueOrDefault(vocabularySet.Theme);

            var petDistribution = new Dictionary<PetRarity, (int Count, double DropRate)>
            {
                { PetRarity.Common,     (10, 0.40) },
                { PetRarity.Uncommon,   (5,  0.25) },
                { PetRarity.Rare,       (3,  0.15) },
                { PetRarity.Epic,       (2,  0.05) },
                { PetRarity.Legendary,  (1,  0.01) }
            };

            foreach (var (rarity, (count, dropRate)) in petDistribution)
            {
                var pets = await _uow.Pet.GetRandomPetsByRarityAsync(rarity, count, themePetTypes, cancellationToken);
                if (pets.Count < count)
                    throw new InvalidOperationException($"Not enough {rarity} pets. Need {count}, found {pets.Count}.");

                vocabularySet.SetRewardPets.AddRange(pets.Select(p => new SetRewardPet
                {
                    PetId = p.Id,
                    DropRate = dropRate
                }));
            }

            await _uow.VocabularySet.CreateVocabularySetAsync(vocabularySet, cancellationToken);

            // Auto-add to creator's library
            var userSetLink = new UserVocabularySet
            {
                UserId = userId,
                VocabularySetId = vocabularySet.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _uow.UserVocabularySet.AddVocabularySetToUserAsync(userSetLink, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);

            InvalidateCachePrefix("VocabularySets_");
            InvalidateCachePrefix($"VocabularySet_{vocabularySet.Id}");

            _logger.LogInformation("Vocabulary set created: ID {SetId} by User {UserId}", vocabularySet.Id, userId);

            return MapToDto(vocabularySet, userId);
        }

        // ============================================================================
        // READ
        // ============================================================================

        /// <summary>
        /// Lấy chi tiết một bộ từ vựng theo ID (có cache 30 phút).
        /// </summary>
        public async Task<VocabularySetDetailDto?> GetVocabularySetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"VocabularySet_{id}";

            if (_cache.TryGetValue(cacheKey, out VocabularySetDetailDto cached))
            {
                _logger.LogDebug("Cache HIT: {CacheKey}", cacheKey);
                return cached;
            }

            var set = await _uow.VocabularySet.GetVocabularySetByIdAsync(id, cancellationToken);
            if (set == null) return null;

            var dto = new VocabularySetDetailDto
            {
                Id = set.Id,
                Title = set.Title,
                Theme = set.Theme,
                ImageUrl = set.ImageUrl,
                Description = set.Description,
                DifficultyLevel = set.DifficultyLevel,
                IsActive = set.IsActive,
                CreatedAt = set.CreatedAt,
                //IsPublic = set.IsPublic,
                //CreatedById = set.CreatedById,
                //CreatedByUsername = set.CreatedBy?.Username,
                VocabularyIds = set.SetVocabularies.OrderBy(sv => sv.Order).Select(sv => sv.VocabularyId).ToList(),
                //TotalVocabularies = set.SetVocabularies.Count
            };

            CacheResult(cacheKey, dto, TimeSpan.FromMinutes(30));
            return dto;
        }

        /// <summary>
        /// Lấy danh sách bộ từ vựng với bộ lọc nâng cao và phân trang (có cache 15 phút).
        /// </summary>
        public async Task<IEnumerable<VocabularySetDto>> GetAllVocabularySetsAsync(
            string? title = null,
            VocabularySetTheme? theme = null,
            VocabularyDifficultyLevel? difficulty = null,
            DateTime? createdAfter = null,
            bool? isOwned = null,
            int? userId = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"VocabularySets_{title ?? "∅"}_{theme}_{difficulty}_{createdAfter:yyyyMMdd}_{isOwned}_{userId ?? 0}_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<VocabularySetDto> cached))
            {
                _logger.LogDebug("Cache HIT: {CacheKey}", cacheKey);
                return cached;
            }

            var sets = await _uow.VocabularySet.GetAllVocabularySetsAsync(
                title, theme, difficulty, createdAfter, isOwned, userId, pageNumber, pageSize, cancellationToken);

            var dtos = sets.Select(s => MapToDto(s, userId)).ToList();

            CacheResult(cacheKey, dtos, TimeSpan.FromMinutes(15));
            return dtos;
        }

        // ============================================================================
        // UPDATE
        // ============================================================================

        /// <summary>
        /// Cập nhật bộ từ vựng (chỉ chủ sở hữu hoặc admin).
        /// </summary>
        public async Task<VocabularySetDto?> UpdateVocabularySetAsync(
            int id,
            UpdateVocabularySetDto dto,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.", nameof(dto.Title));

            _logger.LogInformation("Updating vocabulary set ID {SetId}", id);

            var set = await _uow.VocabularySet.GetVocabularySetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Vocabulary set {id} not found.");

            // Validate vocabularies
            var uniqueIds = dto.VocabularyIds.Distinct().ToList();
            foreach (var vid in uniqueIds)
            {
                if (await _uow.Vocabulary.GetVocabularyByIdAsync(vid, cancellationToken) == null)
                    throw new KeyNotFoundException($"Vocabulary ID {vid} not found.");
            }

            // Update fields
            set.Title = dto.Title.Trim();
            set.Theme = dto.Theme;
            set.Description = dto.Description?.Trim();
            set.DifficultyLevel = dto.DifficultyLevel;
            set.IsActive = dto.IsActive;

            // Replace vocabularies
            set.SetVocabularies = uniqueIds
                .Select((vid, idx) => new SetVocabulary
                {
                    VocabularySetId = id,
                    VocabularyId = vid,
                    Order = idx + 1
                })
                .ToList();

            await _uow.VocabularySet.UpdateVocabularySetAsync(set, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            InvalidateCachePrefix($"VocabularySet_{id}");
            InvalidateCachePrefix("VocabularySets_");

            _logger.LogInformation("Vocabulary set {SetId} updated successfully", id);
            return MapToDto(set);
        }

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Xóa bộ từ vựng (chỉ chủ sở hữu hoặc admin).
        /// </summary>
        public async Task<bool> DeleteVocabularySetAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting vocabulary set ID {SetId}", id);

            var success = await _uow.VocabularySet.DeleteVocabularySetAsync(id, cancellationToken);
            if (success)
            {
                await _uow.SaveChangesAsync(cancellationToken);
                InvalidateCachePrefix($"VocabularySet_{id}");
                InvalidateCachePrefix("VocabularySets_");
                _logger.LogInformation("Vocabulary set {SetId} deleted", id);
            }

            return success;
        }

        // ============================================================================
        // PRIVATE HELPERS
        // ============================================================================

        /// <summary>
        /// Mapping từ VocabularySetTheme → danh sách PetType ưu tiên.
        /// Khi tạo set, hệ thống sẽ ưu tiên chọn pet có type khớp với theme.
        /// Nếu không đủ pet của type đó, tự động fallback về random rarity thường.
        /// </summary>
        private static readonly Dictionary<VocabularySetTheme, IEnumerable<PetType>> ThemeToPetTypesMap = new()
        {
            { VocabularySetTheme.DailyLife,   [PetType.Normal] },
            { VocabularySetTheme.Nature,      [PetType.Grass,    PetType.Bug] },
            { VocabularySetTheme.Weather,     [PetType.Ice,      PetType.Flying] },
            { VocabularySetTheme.Food,        [PetType.Water,    PetType.Fairy] },
            { VocabularySetTheme.Technology,  [PetType.Electric] },
            { VocabularySetTheme.Travel,      [PetType.Flying,   PetType.Normal] },
            { VocabularySetTheme.Health,      [PetType.Fairy,    PetType.Psychic] },
            { VocabularySetTheme.Sports,      [PetType.Fighting] },
            { VocabularySetTheme.Business,    [PetType.Steel] },
            { VocabularySetTheme.Science,     [PetType.Psychic,  PetType.Electric] },
            { VocabularySetTheme.Art,         [PetType.Dragon,   PetType.Psychic] },
            { VocabularySetTheme.Mystery,     [PetType.Ghost] },
            { VocabularySetTheme.Dark,        [PetType.Dark] },
            { VocabularySetTheme.Custom,      [PetType.Fire] },
            { VocabularySetTheme.Challenge,   [PetType.Rock] },
            { VocabularySetTheme.Poison,      [PetType.Poison] },
        };

        private VocabularySetDto MapToDto(VocabularySet set, int? currentUserId = null) => new()
        {
            Id = set.Id,
            Title = set.Title,
            Theme = set.Theme.ToString(),
            Description = set.Description,
            ImageUrl = set.ImageUrl,
            DifficultyLevel = set.DifficultyLevel.ToString(),
            CreatedAt = set.CreatedAt,
            IsActive = set.IsActive,
            IsPublic = set.IsPublic,
            CreatedById = set.CreatedById,
            CreatedByUsername = set.CreatedBy?.Username,
            IsOwned = currentUserId.HasValue &&
                     set.UserVocabularySets.Any(uvs => uvs.UserId == currentUserId.Value && uvs.IsActive),
            VocabularyIds = set.SetVocabularies.OrderBy(sv => sv.Order).Select(sv => sv.VocabularyId).ToList(),
            //TotalVocabularies = set.SetVocabularies.Count
        };

        private void CacheResult<T>(string key, T value, TimeSpan expiration)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration)
                .SetSize(1);

            lock (_cacheLock)
            {
                _cache.Set(key, value, options);
                _cacheKeys.Add(key);
            }
        }

        private void InvalidateCachePrefix(string prefix)
        {
            lock (_cacheLock)
            {
                var keys = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                    _cacheKeys.Remove(key);
                }
                if (keys.Any())
                    _logger.LogDebug("Invalidated {Count} cache keys with prefix {Prefix}", keys.Count, prefix);
            }
        }
    }
}