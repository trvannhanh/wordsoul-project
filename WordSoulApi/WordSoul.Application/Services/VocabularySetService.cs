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
        private readonly IGeminiAiService _geminiAiService;
        private readonly IAzureSpeechService _azureSpeechService;
        private readonly IUnsplashService _unsplashService;
        private readonly IVocabularyAiCacheService _aiCache;

        // Thread-safe cache key tracking
        private readonly HashSet<string> _cacheKeys = [];
        private readonly object _cacheLock = new();

        public VocabularySetService(
            IUnitOfWork uow,
            ILogger<VocabularySetService> logger,
            IMemoryCache cache,
            IGeminiAiService geminiAiService,
            IAzureSpeechService azureSpeechService,
            IUnsplashService unsplashService,
            IVocabularyAiCacheService aiCache)
        {
            _uow = uow;
            _logger = logger;
            _cache = cache;
            _geminiAiService = geminiAiService;
            _azureSpeechService = azureSpeechService;
            _unsplashService = unsplashService;
            _aiCache = aiCache;
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

            await _uow.SaveChangesAsync(cancellationToken);

            
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
        // AI-POWERED CREATE
        // ============================================================================

        /// <summary>
        /// Sinh dữ liệu nháp qua AI cho danh sách từ vựng.
        /// </summary>
        public async Task<List<VocabularyPreviewDto>> AiPreviewVocabularySetAsync(
            AiPreviewRequestDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (dto.Words == null || dto.Words.Count == 0)
                throw new ArgumentException("At least one word is required.", nameof(dto.Words));

            var normalizedWords = dto.Words
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => !string.IsNullOrEmpty(w))
                .Distinct()
                .ToList();

            // Lấy từ vựng hiện có (phân biệt System và Custom của chính user này)
            var existingVocabs = await _uow.Vocabulary.GetVocabulariesByWordsAsync(normalizedWords, userId, cancellationToken);
            var existingWordMap = existingVocabs
                .Where(v => v.Word != null)
                .ToDictionary(v => v.Word!.ToLowerInvariant(), v => v);

            var missingWords = normalizedWords
                .Where(w => !existingWordMap.ContainsKey(w))
                .ToList();

            var resultsMap = new Dictionary<string, VocabularyPreviewDto>();

            // 1. Thêm các từ đã có vào map
            foreach (var vocab in existingVocabs)
            {
                if (vocab.Word == null) continue;
                resultsMap[vocab.Word.ToLowerInvariant()] = new VocabularyPreviewDto
                {
                    Id = vocab.Id,
                    IsExisting = true,
                    IsAiGenerated = false,
                    Word = vocab.Word,
                    Meaning = vocab.Meaning ?? "",
                    Pronunciation = vocab.Pronunciation ?? "",
                    PartOfSpeech = vocab.PartOfSpeech?.ToString() ?? "",
                    CefrLevel = vocab.CEFRLevel?.ToString() ?? "",
                    Description = vocab.Description ?? "",
                    ExampleSentence = vocab.ExampleSentence ?? ""
                };
            }

            // 2. Kiểm tra cache cho các từ còn thiếu
            var wordsNotCached = new List<string>();
            foreach (var word in missingWords)
            {
                var cached = await _aiCache.GetAsync(word, cancellationToken);
                if (cached != null)
                {
                    resultsMap[word] = cached;
                }
                else
                {
                    wordsNotCached.Add(word);
                }
            }

            // 3. Gọi Gemini cho các từ thực sự chưa có (nếu UseAi = true)
            if (wordsNotCached.Count > 0)
            {
                if (dto.UseAi)
                {
                    var aiResults = await _geminiAiService.GenerateVocabularyMetadataAsync(wordsNotCached, cancellationToken);
                    
                    foreach (var aiResult in aiResults)
                    {
                        var wordKey = aiResult.Word.ToLowerInvariant();
                        var previewDto = new VocabularyPreviewDto
                        {
                            Id = null,
                            IsExisting = false,
                            IsAiGenerated = true,
                            Word = aiResult.Word,
                            Meaning = aiResult.Meaning ?? "",
                            Pronunciation = aiResult.Pronunciation ?? "",
                            PartOfSpeech = aiResult.PartOfSpeech ?? "",
                            CefrLevel = aiResult.CefrLevel ?? "",
                            Description = aiResult.Description ?? "",
                            ExampleSentence = aiResult.ExampleSentence ?? ""
                        };
                        
                        resultsMap[wordKey] = previewDto;
                        
                        // Lưu vào cache để lần sau không gọi lại Gemini
                        await _aiCache.SetAsync(wordKey, previewDto, cancellationToken);
                    }
                }
            }

            // 4. Tổng hợp lại theo thứ tự normalizedWords ban đầu
            var previewList = new List<VocabularyPreviewDto>();
            foreach (var word in normalizedWords)
            {
                if (resultsMap.TryGetValue(word, out var res))
                {
                    previewList.Add(res);
                }
                else
                {
                    // Trường hợp AI không trả về hoặc UseAi = false
                    previewList.Add(new VocabularyPreviewDto
                    {
                        Id = null,
                        IsExisting = false,
                        IsAiGenerated = false,
                        Word = word
                    });
                }
            }

            return previewList;
        }

        public async Task<AiCreateVocabularySetResultDto> AiCreateVocabularySetAsync(
            AiCreateVocabularySetDto dto,
            string? imageUrl,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.", nameof(dto.Title));

            if (dto.Vocabularies == null || dto.Vocabularies.Count == 0)
                throw new ArgumentException("At least one word is required.", nameof(dto.Vocabularies));

            _logger.LogInformation("User {UserId} starting AI-create vocabulary set '{Title}' with {Count} words",
                userId, dto.Title, dto.Vocabularies.Count);

            var newlyCreatedVocabs = new List<Vocabulary>();
            var failedWords = new List<string>();
            var allVocabIds = new List<int>();
            int existedCount = 0;

            foreach (var vocabPreview in dto.Vocabularies)
            {
                if (string.IsNullOrWhiteSpace(vocabPreview.Word)) continue;

                var wordKey = vocabPreview.Word.ToLowerInvariant().Trim();

                if (vocabPreview.IsExisting && vocabPreview.Id.HasValue)
                {
                    // Từ đã tồn tại
                    var existingVocab = await _uow.Vocabulary.GetVocabularyByIdAsync(vocabPreview.Id.Value, cancellationToken);
                    if (existingVocab != null)
                    {
                        allVocabIds.Add(existingVocab.Id);
                        existedCount++;
                    }
                    continue;
                }

                // Từ mới -> Gọi API hình ảnh và âm thanh
                try
                {
                    // Unsplash image
                    var fetchedImageUrl = await _unsplashService.GetFirstImageUrlAsync(vocabPreview.Word, cancellationToken);

                    // Azure TTS - word audio
                    var wordAudioBlobName = $"{wordKey}-word.mp3";
                    var wordAudioUrl = await _azureSpeechService.SynthesizeAndUploadAsync(
                        vocabPreview.Word, wordAudioBlobName, cancellationToken);

                    // Azure TTS - example sentence audio
                    string? exampleAudioUrl = null;
                    if (!string.IsNullOrWhiteSpace(vocabPreview.ExampleSentence))
                    {
                        var exampleBlobName = $"{wordKey}-example.mp3";
                        exampleAudioUrl = await _azureSpeechService.SynthesizeAndUploadAsync(
                            vocabPreview.ExampleSentence, exampleBlobName, cancellationToken);
                    }

                    var partOfSpeech = MapPartOfSpeech(vocabPreview.PartOfSpeech);
                    var cefrLevel = MapCefrLevel(vocabPreview.CefrLevel);

                    var vocabulary = new Vocabulary
                    {
                        Word = vocabPreview.Word.Trim(),
                        Meaning = vocabPreview.Meaning?.Trim(),
                        Pronunciation = vocabPreview.Pronunciation?.Trim(),
                        PartOfSpeech = partOfSpeech,
                        CEFRLevel = cefrLevel,
                        Description = vocabPreview.Description?.Trim(),
                        ExampleSentence = vocabPreview.ExampleSentence?.Trim(),
                        ImageUrl = fetchedImageUrl,
                        PronunciationUrl = wordAudioUrl,
                        ExampleSentenceAudioUrl = exampleAudioUrl,
                        IsCustom = !vocabPreview.IsAiGenerated, // Chỉ đánh dấu custom nếu user tự điền (không do AI sinh)
                        CreatorId = userId
                    };

                    await _uow.Vocabulary.CreateVocabularyAsync(vocabulary, cancellationToken);
                    newlyCreatedVocabs.Add(vocabulary);

                    _logger.LogDebug("Created vocabulary: '{Word}' (image={HasImage}, audio={HasAudio}, exAudio={HasExAudio})",
                        vocabPreview.Word, fetchedImageUrl != null, wordAudioUrl != null, exampleAudioUrl != null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process word '{Word}' — skipping", vocabPreview.Word);
                    failedWords.Add(wordKey);
                }
            }

            if (newlyCreatedVocabs.Count > 0)
            {
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Created {Count} new vocabularies", newlyCreatedVocabs.Count);
                allVocabIds.AddRange(newlyCreatedVocabs.Select(v => v.Id));
            }

            allVocabIds = allVocabIds.Distinct().ToList();

            if (allVocabIds.Count == 0)
                throw new InvalidOperationException("No vocabularies could be created or found. Please check the word list.");

            // ── Create VocabularySet ─────────────────────────────────────
            var createSetDto = new CreateVocabularySetDto
            {
                Title = dto.Title,
                Theme = dto.Theme,
                Description = dto.Description,
                DifficultyLevel = dto.DifficultyLevel,
                IsActive = dto.IsActive,
                IsPublic = dto.IsPublic,
                VocabularyIds = allVocabIds
            };

            var vocabularySetDto = await CreateVocabularySetAsync(createSetDto, imageUrl, userId, cancellationToken);

            _logger.LogInformation(
                "AI-create complete: Set={SetId}, Total={Total}, New={New}, Existed={Existed}, Failed={Failed}",
                vocabularySetDto.Id, dto.Vocabularies.Count, newlyCreatedVocabs.Count, existedCount, failedWords.Count);

            return new AiCreateVocabularySetResultDto
            {
                VocabularySet = vocabularySetDto,
                TotalWords = dto.Vocabularies.Count,
                NewlyCreated = newlyCreatedVocabs.Count,
                AlreadyExisted = existedCount,
                FailedWords = failedWords
            };
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
                VocabularyIds = set.SetVocabularies.OrderBy(sv => sv.Order).Select(sv => sv.VocabularyId).ToList(),
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

        /// <summary>
        /// Lấy toàn bộ bộ từ vựng, gom nhóm theo Theme (chủ đề), trả về tối đa N bộ từ mỗi chủ đề.
        /// Chuyên dùng để tối ưu N+1 API gọi từ Frontend.
        /// </summary>
        public async Task<Dictionary<string, List<VocabularySetDto>>> GetGroupedVocabularySetsAsync(
            string? title = null,
            int? userId = null,
            int limitPerTheme = 6,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"VocabularySetsGrouped_{title ?? "∅"}_{userId ?? 0}_{limitPerTheme}";

            if (_cache.TryGetValue(cacheKey, out Dictionary<string, List<VocabularySetDto>> cached))
            {
                _logger.LogDebug("Cache HIT: {CacheKey}", cacheKey);
                return cached;
            }

            // Fetch all matching sets without pagination
            var allSets = await _uow.VocabularySet.GetAllVocabularySetsAsync(
                title, null, null, null, null, userId, 1, 1000, cancellationToken);

            var groupedDtos = allSets
                .GroupBy(s => s.Theme.ToString())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => MapToDto(s, userId))
                          .Take(limitPerTheme)
                          .ToList()
                );

            CacheResult(cacheKey, groupedDtos, TimeSpan.FromMinutes(15));
            return groupedDtos;
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
            { VocabularySetTheme.DailyLife,     [PetType.Normal] },
            { VocabularySetTheme.Nature,        [PetType.Grass] },
            { VocabularySetTheme.Food,          [PetType.Fire] },
            { VocabularySetTheme.Weather,       [PetType.Flying] },
            { VocabularySetTheme.Technology,    [PetType.Electric] },
            { VocabularySetTheme.Travel,        [PetType.Ground] },
            { VocabularySetTheme.Health,        [PetType.Fairy] },
            { VocabularySetTheme.Sports,        [PetType.Fighting] },
            { VocabularySetTheme.Business,      [PetType.Steel] },
            { VocabularySetTheme.Science,       [PetType.Psychic] },
            { VocabularySetTheme.Art,           [PetType.Bug] },
            { VocabularySetTheme.Communication, [PetType.Water] },
            { VocabularySetTheme.Mystery,       [PetType.Ghost] },
            { VocabularySetTheme.Dark,          [PetType.Dark] },
            { VocabularySetTheme.Academic,      [PetType.Ice] },
            { VocabularySetTheme.Challenge,     [PetType.Rock] },
            { VocabularySetTheme.TrapWords,     [PetType.Poison] },
            { VocabularySetTheme.System,        [PetType.Dragon] },
            { VocabularySetTheme.Custom,        [PetType.Normal, PetType.Psychic] }
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

        private static WordSoul.Domain.Enums.PartOfSpeech MapPartOfSpeech(string pos) => pos?.ToLowerInvariant().Trim() switch
        {
            "noun" => WordSoul.Domain.Enums.PartOfSpeech.Noun,
            "verb" => WordSoul.Domain.Enums.PartOfSpeech.Verb,
            "adjective" => WordSoul.Domain.Enums.PartOfSpeech.Adjective,
            "adverb" => WordSoul.Domain.Enums.PartOfSpeech.Adverb,
            "pronoun" => WordSoul.Domain.Enums.PartOfSpeech.Pronoun,
            "preposition" => WordSoul.Domain.Enums.PartOfSpeech.Preposition,
            "conjunction" => WordSoul.Domain.Enums.PartOfSpeech.Conjunction,
            "interjection" => WordSoul.Domain.Enums.PartOfSpeech.Interjection,
            _ => WordSoul.Domain.Enums.PartOfSpeech.Noun
        };

        private static WordSoul.Domain.Enums.CEFRLevel MapCefrLevel(string level) => level?.ToUpperInvariant().Trim() switch
        {
            "A1" => WordSoul.Domain.Enums.CEFRLevel.A1,
            "A2" => WordSoul.Domain.Enums.CEFRLevel.A2,
            "B1" => WordSoul.Domain.Enums.CEFRLevel.B1,
            "B2" => WordSoul.Domain.Enums.CEFRLevel.B2,
            "C1" => WordSoul.Domain.Enums.CEFRLevel.C1,
            "C2" => WordSoul.Domain.Enums.CEFRLevel.C2,
            _ => WordSoul.Domain.Enums.CEFRLevel.A1
        };
    }
}
