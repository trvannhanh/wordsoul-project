using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class VocabularySetService : IVocabularySetService
    {
        private readonly IVocabularySetRepository _vocabularySetRepository;
        private readonly IVocabularyRepository _vocabularyRepository;
        public readonly IUserVocabularySetRepository _userVocabularySetRepository;
        public readonly IUserRepository _userRepository;
        public readonly IPetRepository _petRepository;
        private readonly ILogger<VocabularySetService> _logger;
        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _cacheKeys = new HashSet<string>(); // Theo dõi key cache

        private readonly object _lockObject = new object(); // Đảm bảo thread-safe

        public VocabularySetService(
            IVocabularySetRepository vocabularySetRepository,
            IVocabularyRepository vocabularyRepository,
            ILogger<VocabularySetService> logger,
            IUserVocabularySetRepository userVocabularySetRepository,
            IUserRepository userRepository,
            IPetRepository petRepository, 
            IMemoryCache cache)
        {
            _vocabularySetRepository = vocabularySetRepository;
            _vocabularyRepository = vocabularyRepository;
            _logger = logger;
            _userVocabularySetRepository = userVocabularySetRepository;
            _userRepository = userRepository;
            _petRepository = petRepository;
            _cache = cache;
        }
        //------------------------------ CREATE -----------------------------------------

        // Tạo bộ từ vựng mới
        public async Task<VocabularySetDto> CreateVocabularySetAsync(CreateVocabularySetDto createDto, string? imageUrl, int userId)
        {
            _logger.LogInformation("Creating vocabulary set with title: {Title} by user ID: {UserId}", createDto.Title, userId);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User with ID: {UserId} not found", userId);
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            if (string.IsNullOrWhiteSpace(createDto.Title))
            {
                _logger.LogError("Title is required for creating a vocabulary set");
                throw new ArgumentException("Title is required.", nameof(createDto.Title));
            }

            var uniqueVocabIds = createDto.VocabularyIds.Distinct().ToList();
            if (uniqueVocabIds.Count != createDto.VocabularyIds.Count)
            {
                _logger.LogWarning("Duplicate VocabularyIds detected for user {UserId}", userId);
                throw new ArgumentException("Vocabulary IDs must be unique.");
            }
            if (uniqueVocabIds.Count > 50)
            {
                _logger.LogWarning("Too many VocabularyIds ({Count}) for user {UserId}", uniqueVocabIds.Count, userId);
                throw new ArgumentException("Maximum 50 vocabularies per set.");
            }

            foreach (var vocabId in uniqueVocabIds)
            {
                var vocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(vocabId);
                if (vocabulary == null)
                {
                    _logger.LogError("Vocabulary with ID: {VocabularyId} not found", vocabId);
                    throw new KeyNotFoundException($"Vocabulary with ID {vocabId} not found.");
                }
            }

            var vocabularySet = new VocabularySet
            {
                Title = createDto.Title,
                Theme = createDto.Theme,
                ImageUrl = imageUrl,
                Description = createDto.Description,
                DifficultyLevel = createDto.DifficultyLevel,
                IsActive = createDto.IsActive,
                IsPublic = createDto.IsPublic,
                CreatedById = userId,
                SetVocabularies = uniqueVocabIds.Select((vocabId, index) => new SetVocabulary
                {
                    VocabularyId = vocabId,
                    Order = index + 1
                }).ToList(),
                SetRewardPets = new List<SetRewardPet>()
            };

            var petDistribution = new Dictionary<PetRarity, (int Count, double DropRate)>
    {
                { PetRarity.Common, (10, 0.4) },
                { PetRarity.Uncommon, (5, 0.25) },
                { PetRarity.Rare, (3, 0.15) },
                { PetRarity.Epic, (2, 0.05) },
                { PetRarity.Legendary, (1, 0.01) }
    };

            foreach (var (rarity, (count, dropRate)) in petDistribution)
            {
                var pets = await _petRepository.GetRandomPetsByRarityAsync(rarity, count);
                if (pets.Count < count)
                {
                    _logger.LogError("Not enough pets with rarity {Rarity}. Requested: {Count}, Found: {Found}", rarity, count, pets.Count);
                    throw new InvalidOperationException($"Not enough pets with rarity {rarity}. Required: {count}, Found: {pets.Count}");
                }

                vocabularySet.SetRewardPets.AddRange(pets.Select(pet => new SetRewardPet
                {
                    PetId = pet.Id,
                    DropRate = dropRate
                }));
            }

            try
            {
                var createdVocabularySet = await _vocabularySetRepository.CreateVocabularySetAsync(vocabularySet);

                var userVocabularySet = new UserVocabularySet
                {
                    UserId = userId,
                    VocabularySetId = createdVocabularySet.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _userVocabularySetRepository.AddVocabularySetToUserAsync(userVocabularySet);

                // Xóa cache danh sách sau khi tạo
                RemoveCacheByPrefix("VocabularySets_");
                _logger.LogInformation("Vocabulary set created with ID: {Id}", createdVocabularySet.Id);
                return MapToDto(createdVocabularySet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create vocabulary set for user {UserId}", userId);
                throw;
            }
        }


        //------------------------------ READ -------------------------------------------
        // Lấy bộ từ vựng theo ID
        public async Task<VocabularySetDetailDto?> GetVocabularySetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving vocabulary set with ID: {Id}", id);
            var cacheKey = $"VocabularySet_{id}";

            if (_cache.TryGetValue(cacheKey, out VocabularySetDetailDto cachedResult))
            {
                _logger.LogDebug("Cache hit for vocabulary set ID: {Id}", id);
                return cachedResult;
            }

            var vocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(id);
            if (vocabularySet == null)
            {
                _logger.LogWarning("Vocabulary set with ID: {Id} not found", id);
                return null;
            }

            var result = MapToDetailDto(vocabularySet);
            lock (_lockObject)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
                _cacheKeys.Add(cacheKey); // Theo dõi key
            }
            _logger.LogDebug("Cache miss, stored vocabulary set ID: {Id}", id);
            return result;
        }

        public async Task<IEnumerable<VocabularySetDto>> GetAllVocabularySetsAsync(
        string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty, DateTime? createdAfter,
        bool? isOwned, int? userId, int pageNumber, int pageSize)
        {
            var cacheKey = $"VocabularySets_{title ?? "null"}_{theme ?? null}_{difficulty ?? null}_{createdAfter ?? DateTime.MinValue}_{isOwned ?? false}_{userId ?? 0}_{pageNumber}_{pageSize}";
            _logger.LogInformation("Retrieving vocabulary sets with key: {Key}", cacheKey);

            if (_cache.TryGetValue(cacheKey, out IEnumerable<VocabularySetDto> cachedResult))
            {
                _logger.LogDebug("Cache hit for vocabulary sets with key: {Key}", cacheKey);
                return cachedResult;
            }

            var sets = await _vocabularySetRepository.GetAllVocabularySetsAsync(title, theme, difficulty, createdAfter, isOwned, userId, pageNumber, pageSize);
            var vocabularySetDtos = sets.Select(vs => MapToDto(vs, userId)).ToList();

            lock (_lockObject)
            {
                _cache.Set(cacheKey, vocabularySetDtos, TimeSpan.FromMinutes(15));
                _cacheKeys.Add(cacheKey); // Theo dõi key
            }
            _logger.LogDebug("Cache miss, stored vocabulary sets with key: {Key}", cacheKey);
            return vocabularySetDtos;
        }

        // ------------------------------- UPDATE -------------------------------------------

        // Cập nhật bộ từ vựng
        public async Task<VocabularySetDto?> UpdateVocabularySetAsync(int id, UpdateVocabularySetDto updateDto)
        {
            _logger.LogInformation("Updating vocabulary set with ID: {Id}", id);

            if (string.IsNullOrWhiteSpace(updateDto.Title))
            {
                _logger.LogError("Title is required for updating a vocabulary set");
                throw new ArgumentException("Title is required.", nameof(updateDto.Title));
            }

            foreach (var vocabId in updateDto.VocabularyIds)
            {
                var vocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(vocabId);
                if (vocabulary == null)
                {
                    _logger.LogError("Vocabulary with ID: {VocabularyId} not found", vocabId);
                    throw new KeyNotFoundException($"Vocabulary with ID {vocabId} not found.");
                }
            }

            var existingVocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(id);
            if (existingVocabularySet == null)
            {
                _logger.LogWarning("Vocabulary set with ID: {Id} not found", id);
                return null;
            }

            existingVocabularySet.Title = updateDto.Title;
            existingVocabularySet.Theme = updateDto.Theme;
            existingVocabularySet.Description = updateDto.Description;
            existingVocabularySet.DifficultyLevel = updateDto.DifficultyLevel;
            existingVocabularySet.IsActive = updateDto.IsActive;
            existingVocabularySet.SetVocabularies = updateDto.VocabularyIds.Select((vocabId, index) => new SetVocabulary
            {
                VocabularySetId = id,
                VocabularyId = vocabId,
                Order = index + 1
            }).ToList();

            var updatedVocabularySet = await _vocabularySetRepository.UpdateVocabularySetAsync(existingVocabularySet);
            if (updatedVocabularySet == null)
            {
                _logger.LogWarning("Failed to update vocabulary set with ID: {Id}", id);
                return null;
            }

            // Xóa cache chi tiết và danh sách
            RemoveCacheByPrefix($"VocabularySet_{id}");
            RemoveCacheByPrefix("VocabularySets_");
            _logger.LogInformation("Vocabulary set updated with ID: {Id}", updatedVocabularySet.Id);
            return MapToDto(updatedVocabularySet);
        }

        //------------------------------ DELETE -----------------------------------------
        // Xóa bộ từ vựng theo ID
        public async Task<bool> DeleteVocabularySetAsync(int id)
        {
            _logger.LogInformation("Deleting vocabulary set with ID: {Id}", id);
            var result = await _vocabularySetRepository.DeleteVocabularySetAsync(id);
            if (!result)
            {
                _logger.LogWarning("Vocabulary set with ID: {Id} not found", id);
            }
            else
            {
                // Xóa cache chi tiết và danh sách
                RemoveCacheByPrefix($"VocabularySet_{id}");
                RemoveCacheByPrefix("VocabularySets_");
                _logger.LogInformation("Vocabulary set with ID: {Id} deleted", id);
            }
            return result;
        }


        //------------------------------ MAPPING -----------------------------------------
        private VocabularySetDto MapToDto(VocabularySet vocabularySet)
        {
            return new VocabularySetDto
            {
                Id = vocabularySet.Id,
                Title = vocabularySet.Title,
                Theme = vocabularySet.Theme.ToString(),
                Description = vocabularySet.Description,
                ImageUrl = vocabularySet.ImageUrl,
                DifficultyLevel = vocabularySet.DifficultyLevel.ToString(),
                CreatedAt = vocabularySet.CreatedAt,
                IsActive = vocabularySet.IsActive,
                CreatedById = vocabularySet.CreatedById,
                CreatedByUsername = vocabularySet.CreatedBy?.Username, // Lấy username nếu có
                IsPublic = vocabularySet.IsPublic,
                VocabularyIds = vocabularySet.SetVocabularies.Select(sv => sv.VocabularyId).ToList()
            };
        }

        private VocabularySetDetailDto MapToDetailDto(VocabularySet vocabularySet)
        {
            return new VocabularySetDetailDto
            {
                Id = vocabularySet.Id,
                Title = vocabularySet.Title,
                Theme = vocabularySet.Theme,
                ImageUrl = vocabularySet.ImageUrl,
                Description = vocabularySet.Description,
                DifficultyLevel = vocabularySet.DifficultyLevel,
                IsActive = vocabularySet.IsActive,
                CreatedAt = vocabularySet.CreatedAt,
                VocabularyIds = vocabularySet.SetVocabularies.Select(sv => sv.VocabularyId).ToList()
            };
        }


        private VocabularySetDto MapToDto(VocabularySet vocabularySet, int? userId)
        {
            return new VocabularySetDto
            {
                Id = vocabularySet.Id,
                Title = vocabularySet.Title,
                Theme = vocabularySet.Theme.ToString(),
                Description = vocabularySet.Description,
                ImageUrl = vocabularySet.ImageUrl,
                DifficultyLevel = vocabularySet.DifficultyLevel.ToString(),
                CreatedAt = vocabularySet.CreatedAt,
                IsActive = vocabularySet.IsActive,
                CreatedById = vocabularySet.CreatedById,
                CreatedByUsername = vocabularySet.CreatedBy?.Username,
                IsPublic = vocabularySet.IsPublic,
                IsOwned = userId.HasValue && vocabularySet.UserVocabularySets.Any(uvs => uvs.UserId == userId.Value && uvs.IsActive), // Mới: false nếu chưa đăng nhập
                VocabularyIds = vocabularySet.SetVocabularies.Select(sv => sv.VocabularyId).ToList()
            };
        }

        // Phương thức xóa cache theo tiền tố
        private void RemoveCacheByPrefix(string prefix)
        {
            lock (_lockObject)
            {
                var keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _cacheKeys.Remove(key);
                    _logger.LogDebug("Removed cache key: {Key}", key);
                }
            }
        }

    }
}
