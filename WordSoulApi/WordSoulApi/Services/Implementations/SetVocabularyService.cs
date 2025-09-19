using Microsoft.Extensions.Caching.Memory;
using WordSoulApi.Models.DTOs;
using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class SetVocabularyService : ISetVocabularyService
    {

        private readonly ISetVocabularyRepository _setVocabularyRepository;
        private readonly IVocabularySetRepository _vocabularySetRepository;
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly ILogger<SetVocabularyService> _logger;

        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _cacheKeys = new HashSet<string>(); // Theo dõi key cache

        private readonly object _lockObject = new object(); // Đảm bảo thread-safe

        public SetVocabularyService(ISetVocabularyRepository setVocabularyRepository, 
                                    ILogger<SetVocabularyService> logger, 
                                    IVocabularySetRepository vocabularySetRepository, 
                                    IVocabularyRepository vocabularyRepository, 
                                    IMemoryCache cache)
        {
            _setVocabularyRepository = setVocabularyRepository;
            _logger = logger;
            _vocabularySetRepository = vocabularySetRepository;
            _vocabularyRepository = vocabularyRepository;
            _cache = cache;
        }


        // -------------------------------------CREATE-------------------------------------------
        // Thêm từ vựng mới vào bộ từ vựng
        public async Task<AdminVocabularyDto?> AddVocabularyToSetAsync(int setId, CreateVocabularyInSetDto vocabularyDto, string? imageUrl)
        {
            _logger.LogInformation("Adding vocabulary to set ID: {SetId}, Word: {Word}", setId, vocabularyDto.Word);

            var vocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(setId);
            if (vocabularySet == null)
            {
                _logger.LogWarning("Vocabulary set with ID: {SetId} not found", setId);
                throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");
            }

            var existingLink = await _setVocabularyRepository.CheckVocabularyExistFromSetAsync(vocabularyDto.Word, setId);
            if (existingLink)
            {
                _logger.LogWarning("Vocabulary {Word} already exists in set ID: {SetId}", vocabularyDto.Word, setId);
                throw new ArgumentException("Từ vựng đã tồn tại trong bộ này.");
            }

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

            _logger.LogDebug("Creating new vocabulary for word: {Word}", vocabularyDto.Word);
            await _vocabularyRepository.CreateVocabularyAsync(vocabulary);

            var maxOrderValue = await _setVocabularyRepository.GetVocabularyOrderMaxAsync(setId);
            var newOrder = maxOrderValue + 1;

            var setVocabulary = new SetVocabulary
            {
                VocabularySetId = setId,
                VocabularyId = vocabulary.Id,
                Order = newOrder
            };

            _logger.LogDebug("Creating link for vocabulary ID: {VocabId} in set ID: {SetId}", vocabulary.Id, setId);
            await _setVocabularyRepository.CreateSetVocabularyAsync(setVocabulary);

            // Xóa cache liên quan đến setId
            RemoveCacheByPrefix($"VocabulariesInSet_{setId}_");
            RemoveCacheByPrefix($"VocabularySetFull_{setId}_");
            _logger.LogInformation("Successfully added vocabulary ID: {VocabId} to set ID: {SetId}", vocabulary.Id, setId);

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

        //-------------------------------------READ-------------------------------------------
        // Lấy danh sách từ vựng trong bộ với phân trang
        public async Task<PagedResult<VocabularyDto>> GetVocabulariesInSetAsync(int setId, int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("Retrieving vocabularies for set ID: {SetId}, Page: {Page}, PageSize: {PageSize}", setId, pageNumber, pageSize);
            var cacheKey = $"VocabulariesInSet_{setId}_{pageNumber}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out PagedResult<VocabularyDto> cachedResult))
            {
                _logger.LogDebug("Cache hit for vocabularies in set ID: {SetId}, Page: {Page}", setId, pageNumber);
                return cachedResult;
            }

            var vocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(setId);
            if (vocabularySet == null)
                throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");

            var (vocabularies, totalCount) = await _setVocabularyRepository.GetVocabulariesFromSetAsync(setId, pageNumber, pageSize);

            var vocabularyDtos = vocabularies.Select(v => new VocabularyDto
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
                Items = vocabularyDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            lock (_lockObject)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
                _cacheKeys.Add(cacheKey);
            }
            _logger.LogDebug("Cache miss, stored vocabularies for set ID: {SetId}, Page: {Page}", setId, pageNumber);
            return result;
        }

        // Lấy bộ từ vựng theo ID kèm chi tiết các từ vựng bên trong với phân trang
        public async Task<VocabularySetFullDetailDto?> GetVocabularySetFullDetailsAsync(int id, int page, int pageSize)
        {
            _logger.LogInformation("Retrieving full details for vocabulary set with ID: {Id}, Page: {Page}, PageSize: {PageSize}", id, page, pageSize);
            var cacheKey = $"VocabularySetFull_{id}_{page}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out VocabularySetFullDetailDto cachedResult))
            {
                _logger.LogDebug("Cache hit for full details of set ID: {Id}, Page: {Page}", id, page);
                return cachedResult;
            }

            var vocabularySet = await _setVocabularyRepository.GetVocabularySetFullDetailsAsync(id, page, pageSize);
            if (vocabularySet == null)
            {
                _logger.LogWarning("Vocabulary set with ID: {Id} not found", id);
                return null;
            }

            var result = await MapToFullDetailDtoAsync(vocabularySet, page, pageSize);

            lock (_lockObject)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
                _cacheKeys.Add(cacheKey);
            }
            _logger.LogDebug("Cache miss, stored full details for set ID: {Id}, Page: {Page}", id, page);
            return result;
        }

        //-------------------------------------DELETE-------------------------------------------
        // Xóa liên kết từ vựng khỏi bộ
        public async Task<bool> RemoveVocabularyFromSetAsync(int setId, int vocabId)
        {
            _logger.LogInformation("Removing vocabulary ID: {VocabId} from set ID: {SetId}", vocabId, setId);

            var setVocabulary = await _setVocabularyRepository.GetSetVocabularyAsync(vocabId, setId);
            if (setVocabulary == null)
            {
                _logger.LogWarning("Link not found for vocabulary ID: {VocabId} in set ID: {SetId}", vocabId, setId);
                return false;
            }

            var result = await _setVocabularyRepository.DeleteSetVocabularyAsync(setVocabulary);
            if (result)
            {
                // Xóa cache liên quan đến setId
                RemoveCacheByPrefix($"VocabulariesInSet_{setId}_");
                RemoveCacheByPrefix($"VocabularySetFull_{setId}_");
                _logger.LogInformation("Successfully removed vocabulary ID: {VocabId} from set ID: {SetId}", vocabId, setId);
            }
            return result;
        }




        // Helper method to map VocabularySet to VocabularySetFullDetailDto
        private async Task<VocabularySetFullDetailDto> MapToFullDetailDtoAsync(VocabularySet vocabularySet, int page, int pageSize)
        {
            var totalVocabularies = await _setVocabularyRepository.CountVocabulariesInSetAsync(vocabularySet.Id);
            return new VocabularySetFullDetailDto
            {
                Id = vocabularySet.Id,
                Title = vocabularySet.Title,
                Theme = vocabularySet.Theme.ToString(),
                ImageUrl = vocabularySet.ImageUrl,
                Description = vocabularySet.Description,
                DifficultyLevel = vocabularySet.DifficultyLevel.ToString(),
                IsActive = vocabularySet.IsActive,
                CreatedAt = vocabularySet.CreatedAt,
                Vocabularies = vocabularySet.SetVocabularies.Select(sv => new VocabularyDetailDto
                {
                    Id = sv.VocabularyId,
                    Word = sv.Vocabulary.Word,
                    Meaning = sv.Vocabulary.Meaning,
                    ImageUrl = sv.Vocabulary.ImageUrl,
                    Pronunciation = sv.Vocabulary.Pronunciation,
                    PartOfSpeech = sv.Vocabulary.PartOfSpeech.ToString()
                }).ToList(),
                TotalVocabularies = totalVocabularies,
                CurrentPage = page,
                PageSize = pageSize
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
