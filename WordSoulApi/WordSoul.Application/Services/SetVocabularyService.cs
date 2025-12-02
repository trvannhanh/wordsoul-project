using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.DTOs.VocabularySet;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class SetVocabularyService : ISetVocabularyService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SetVocabularyService> _logger;
        private readonly IMemoryCache _cache;

        // Theo dõi các key cache để xóa theo prefix (thread-safe)
        private readonly HashSet<string> _cacheKeys = new();
        private readonly object _lockObject = new();

        public SetVocabularyService(
            IUnitOfWork uow,
            ILogger<SetVocabularyService> logger,
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
        /// Thêm một từ vựng mới vào bộ từ vựng. Nếu từ đã tồn tại trong bộ thì ném lỗi.
        /// </summary>
        /// <param name="setId">ID của bộ từ vựng.</param>
        /// <param name="vocabularyDto">Dữ liệu từ vựng cần thêm.</param>
        /// <param name="imageUrl">URL hình ảnh đã upload (nếu có).</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>AdminVocabularyDto của từ vừa được tạo.</returns>
        /// <exception cref="KeyNotFoundException">Khi bộ từ vựng không tồn tại.</exception>
        /// <exception cref="ArgumentException">Khi từ vựng đã tồn tại trong bộ.</exception>
        public async Task<AdminVocabularyDto?> AddVocabularyToSetAsync(
            int setId,
            CreateVocabularyInSetDto vocabularyDto,
            string? imageUrl,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding vocabulary '{Word}' to set ID {SetId}", vocabularyDto.Word, setId);

            // Kiểm tra bộ từ vựng có tồn tại không
            var vocabularySet = await _uow.VocabularySet.GetVocabularySetByIdAsync(setId, cancellationToken)
                ?? throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");

            // Kiểm tra từ đã tồn tại trong bộ chưa
            var exists = await _uow.SetVocabulary.CheckVocabularyExistFromSetAsync(vocabularyDto.Word, setId, cancellationToken);
            if (exists)
                throw new ArgumentException("Từ vựng đã tồn tại trong bộ này.");

            // Tạo từ vựng mới
            var vocabulary = new Vocabulary
            {
                Word = vocabularyDto.Word,
                Meaning = vocabularyDto.Meaning,
                Pronunciation = vocabularyDto.Pronunciation,
                PartOfSpeech = vocabularyDto.PartOfSpeech,
                Description = vocabularyDto.Description,
                CEFRLevel = vocabularyDto.CEFRLevel,
                ExampleSentence = vocabularyDto.ExampleSentence,
                ImageUrl = imageUrl,
                PronunciationUrl = vocabularyDto.PronunciationUrl
            };

            await _uow.Vocabulary.CreateVocabularyAsync(vocabulary, cancellationToken);

            // Tạo liên kết SetVocabulary với thứ tự tăng dần
            var maxOrder = await _uow.SetVocabulary.GetVocabularyOrderMaxAsync(setId, cancellationToken);
            var setVocabulary = new SetVocabulary
            {
                VocabularySetId = setId,
                VocabularyId = vocabulary.Id,
                Order = maxOrder + 1
            };

            await _uow.SetVocabulary.CreateSetVocabularyAsync(setVocabulary, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // Xóa cache liên quan
            InvalidateCacheByPrefix($"VocabulariesInSet_{setId}_");
            InvalidateCacheByPrefix($"VocabularySetFull_{setId}_");

            _logger.LogInformation("Successfully added vocabulary ID {VocabId} to set ID {SetId}", vocabulary.Id, setId);

            return new AdminVocabularyDto
            {
                Id = vocabulary.Id,
                Word = vocabulary.Word,
                Meaning = vocabulary.Meaning,
                Pronunciation = vocabulary.Pronunciation,
                PartOfSpeech = vocabulary.PartOfSpeech.ToString(),
                CEFRLevel = vocabulary.CEFRLevel.ToString(),
                Description = vocabulary.Description,
                ExampleSentence = vocabulary.ExampleSentence,
                ImageUrl = vocabulary.ImageUrl,
                PronunciationUrl = vocabulary.PronunciationUrl
            };
        }

        // ============================================================================
        // READ
        // ============================================================================

        /// <summary>
        /// Lấy danh sách từ vựng trong một bộ với phân trang (có cache 15 phút).
        /// </summary>
        public async Task<PagedResult<VocabularyDto>> GetVocabulariesInSetAsync(
            int setId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Get vocabularies in set {SetId} - Page {Page}/{PageSize}", setId, pageNumber, pageSize);

            var cacheKey = $"VocabulariesInSet_{setId}_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out PagedResult<VocabularyDto> cached))
            {
                _logger.LogDebug("Cache HIT: {CacheKey}", cacheKey);
                return cached;
            }

            // Kiểm tra bộ có tồn tại
            var setExists = await _uow.VocabularySet.GetVocabularySetByIdAsync(setId, cancellationToken)
                ?? throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");

            var (vocabularies, totalCount) = await _uow.SetVocabulary.GetVocabulariesFromSetAsync(
                setId, pageNumber, pageSize, cancellationToken);

            var dtos = vocabularies.Select(v => new VocabularyDto
            {
                Id = v.Id,
                Word = v.Word,
                Meaning = v.Meaning,
                PartOfSpeech = v.PartOfSpeech.ToString(),
                CEFRLevel = v.CEFRLevel.ToString(),
                Description = v.Description,
                ExampleSentence = v.ExampleSentence,
                ImageUrl = v.ImageUrl,
                PronunciationUrl = v.PronunciationUrl
            }).ToList();

            var result = new PagedResult<VocabularyDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            //Cache kết quả
            lock (_lockObject)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
                _cacheKeys.Add(cacheKey);
            }

            _logger.LogDebug("Cache MISS & STORED: {CacheKey}", cacheKey);
            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết đầy đủ của bộ từ vựng kèm danh sách từ (có phân trang + cache).
        /// </summary>
        public async Task<VocabularySetFullDetailDto?> GetVocabularySetFullDetailsAsync(
            int id,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"VocabularySetFull_{id}_{page}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out VocabularySetFullDetailDto cached))
            {
                _logger.LogDebug("Cache HIT: {CacheKey}", cacheKey);
                return cached;
            }

            var vocabularySet = await _uow.SetVocabulary.GetVocabularySetFullDetailsAsync(id, page, pageSize, cancellationToken);
            if (vocabularySet == null)
                return null;

            var totalVocabularies = await _uow.SetVocabulary.CountVocabulariesInSetAsync(id, cancellationToken);

            var result = new VocabularySetFullDetailDto
            {
                Id = vocabularySet.Id,
                Title = vocabularySet.Title,
                Theme = vocabularySet.Theme.ToString(),
                ImageUrl = vocabularySet.ImageUrl,
                Description = vocabularySet.Description,
                DifficultyLevel = vocabularySet.DifficultyLevel.ToString(),
                IsActive = vocabularySet.IsActive,
                CreatedAt = vocabularySet.CreatedAt,
                TotalVocabularies = totalVocabularies,
                CurrentPage = page,
                PageSize = pageSize,
                Vocabularies = vocabularySet.SetVocabularies.Select(sv => new VocabularyDetailDto
                {
                    Id = sv.VocabularyId,
                    Word = sv.Vocabulary.Word,
                    Meaning = sv.Vocabulary.Meaning,
                    ImageUrl = sv.Vocabulary.ImageUrl,
                    Pronunciation = sv.Vocabulary.Pronunciation,
                    PartOfSpeech = sv.Vocabulary.PartOfSpeech.ToString()
                }).ToList()
            };

            lock (_lockObject)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
                _cacheKeys.Add(cacheKey);
            }

            _logger.LogDebug("Cache MISS & STORED: {CacheKey}", cacheKey);
            return result;
        }

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Xóa liên kết từ vựng khỏi bộ từ vựng (không xóa từ vựng thật).
        /// </summary>
        /// <returns>true nếu xóa thành công, false nếu không tìm thấy liên kết.</returns>
        public async Task<bool> RemoveVocabularyFromSetAsync(
            int setId,
            int vocabId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Removing vocabulary {VocabId} from set {SetId}", vocabId, setId);

            var link = await _uow.SetVocabulary.GetSetVocabularyAsync(vocabId, setId, cancellationToken);
            if (link == null)
            {
                _logger.LogWarning("Link not found: Vocab {VocabId} in Set {SetId}", vocabId, setId);
                return false;
            }

            var success = await _uow.SetVocabulary.DeleteSetVocabularyAsync(link, cancellationToken);
            if (success)
            {
                await _uow.SaveChangesAsync(cancellationToken);

                // Xóa cache liên quan
                InvalidateCacheByPrefix($"VocabulariesInSet_{setId}_");
                InvalidateCacheByPrefix($"VocabularySetFull_{setId}_");

                _logger.LogInformation("Successfully removed vocabulary {VocabId} from set {SetId}", vocabId, setId);
            }

            return success;
        }

        // ============================================================================
        // PRIVATE HELPERS
        // ============================================================================

        /// <summary>
        /// Xóa tất cả các key cache có tiền tố nhất định (thread-safe).
        /// </summary>
        private void InvalidateCacheByPrefix(string prefix)
        {
            lock (_lockObject)
            {
                var keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _cacheKeys.Remove(key);
                    _logger.LogDebug("Invalidated cache key: {Key}", key);
                }
            }
        }
    }
}