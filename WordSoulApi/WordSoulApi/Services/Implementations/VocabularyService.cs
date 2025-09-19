using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WordSoulApi.Models.DTOs;
using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class VocabularyService : IVocabularyService
    {
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<VocabularySetService> _logger;

        public VocabularyService(IVocabularyRepository vocabularyRepository, ILogger<VocabularySetService> logger, IMemoryCache cache)
        {
            _vocabularyRepository = vocabularyRepository;
            _logger = logger;
            _cache = cache;
        }

        //------------------------------ CREATE -----------------------------------------

        // Tạo từ vựng mới
        public async Task<AdminVocabularyDto> CreateVocabularyAsync(CreateVocabularyDto vocabularyDto, string? imageUrl)
        {
            _logger.LogInformation("Creating vocabulary with word: {Word}", vocabularyDto.Word);

            if (string.IsNullOrWhiteSpace(vocabularyDto.Word))
            {
                _logger.LogError("Word is required for creating a vocabulary");
                throw new ArgumentException("Title is required.", nameof(vocabularyDto.Word));
            }

            var vocabulary = new Vocabulary
            {
                Word = vocabularyDto.Word,
                Meaning = vocabularyDto.Meaning,
                Pronunciation = vocabularyDto.Pronunciation,
                PartOfSpeech = vocabularyDto.PartOfSpeech,
                CEFRLevel = vocabularyDto.CEFRLevel,
                Description = vocabularyDto.Description,
                ExampleSentence = vocabularyDto.ExampleSentence,
                ImageUrl = imageUrl,
                PronunciationUrl = vocabularyDto.PronunciationUrl
            };

            try
            {
                var createdVocabulary = await _vocabularyRepository.CreateVocabularyAsync(vocabulary);
                _logger.LogInformation("Vocabulary created with ID: {Id}", createdVocabulary.Id);
                return new AdminVocabularyDto
                {
                    Id = createdVocabulary.Id,
                    Word = createdVocabulary.Word,
                    Meaning = createdVocabulary.Meaning,
                    Pronunciation = createdVocabulary.Pronunciation,
                    PartOfSpeech = createdVocabulary.PartOfSpeech.ToString(),
                    CEFRLevel = createdVocabulary.CEFRLevel.ToString(),
                    Description = createdVocabulary.Description,
                    ExampleSentence = createdVocabulary.ExampleSentence,
                    ImageUrl = createdVocabulary.ImageUrl,
                    PronunciationUrl = createdVocabulary.PronunciationUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create vocabulary with word: {Word}", vocabularyDto.Word);
                throw;
            }
        }



        //------------------------------ READ -------------------------------------------

        public async Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber, int pageSize)
        {
            var cacheKey = $"Vocabularies_{word}_{partOfSpeech}_{cEFRLevel}_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<VocabularyDto> cachedVocabularies))
            {
                _logger.LogDebug("Cache hit for vocabularies with filter: {Key}", cacheKey);
                return cachedVocabularies;
            }

            var vocabularies = await _vocabularyRepository.GetAllVocabulariesAsync(word, meaning, partOfSpeech, cEFRLevel, pageNumber, pageSize);
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

            _cache.Set(cacheKey, vocabularyDtos, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                Size = 1024 // Giới hạn kích thước (bytes)
            });
            _logger.LogDebug("Cache miss, stored vocabularies with filter: {Key}", cacheKey);
            return vocabularyDtos;
        }

        // Lấy từ vựng theo ID
        public async Task<VocabularyDto?> GetVocabularyByIdAsync(int id)
        {
            _logger.LogInformation("Fetching vocabulary with ID: {Id}", id);
            var cacheKey = $"Vocabulary_{id}";
            if (_cache.TryGetValue(cacheKey, out VocabularyDto cachedVocabulary))
            {
                _logger.LogDebug("Cache hit for vocabulary ID: {Id}", id);
                return cachedVocabulary;
            }

            var vocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(id);
            if (vocabulary == null)
            {
                _logger.LogWarning("No vocabulary found with ID: {Id}", id);
                return null;
            }

            var vocabularyDto = new VocabularyDto
            {
                Id = vocabulary.Id,
                Word = vocabulary.Word,
                Meaning = vocabulary.Meaning,
                PartOfSpeech = vocabulary.PartOfSpeech.ToString(),
                CEFRLevel = vocabulary.CEFRLevel.ToString(),
                Description = vocabulary.Description,
                ExampleSentence = vocabulary.ExampleSentence,
                ImageUrl = vocabulary.ImageUrl,
                PronunciationUrl = vocabulary.PronunciationUrl
            };
            _cache.Set(cacheKey, vocabularyDto, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                Size = 1024
            });
            _logger.LogDebug("Cache miss, stored vocabulary ID: {Id}", id);
            return vocabularyDto;
        }

        // Lấy các từ vựng theo danh sách từ
        public async Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(SearchVocabularyDto searchVocabularyDto)
        {
            if (searchVocabularyDto == null || searchVocabularyDto.Words == null || !searchVocabularyDto.Words.Any())
                return new List<VocabularyDto>();

            var cacheKey = $"VocabulariesByWords_{string.Join("_", searchVocabularyDto.Words.OrderBy(w => w))}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<VocabularyDto> cachedVocabularies))
            {
                _logger.LogDebug("Cache hit for vocabularies with words: {Key}", cacheKey);
                return cachedVocabularies;
            }

            var vocabularies = await _vocabularyRepository.GetVocabulariesByWordsAsync(searchVocabularyDto.Words);
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
                Pronunciation = v.Pronunciation,
                PronunciationUrl = v.PronunciationUrl
            }).ToList();

            _cache.Set(cacheKey, vocabularyDtos, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                Size = 1024 // Giới hạn kích thước (bytes)
            });
            _logger.LogDebug("Cache miss, stored vocabularies with words: {Key}", cacheKey);
            return vocabularyDtos;
        }


        //------------------------------ UPDATE -----------------------------------------
        // Cập nhật từ vựng
        public async Task<AdminVocabularyDto> UpdateVocabularyAsync(int id, CreateVocabularyDto vocabularyDto, string? imageUrl)
        {
            _logger.LogInformation("Updating vocabulary with ID: {Id}", id);
            if (string.IsNullOrWhiteSpace(vocabularyDto.Word))
            {
                _logger.LogError("Title is required for updating a vocabulary set");
                throw new ArgumentException("Title is required.", nameof(vocabularyDto.Word));
            }

            var existingVocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(id);
            if (existingVocabulary == null)
            {
                _logger.LogWarning("Vocabulary set with ID: {Id} not found", id);
                return null;
            }

            existingVocabulary.Word = vocabularyDto.Word;
            existingVocabulary.Meaning = vocabularyDto.Meaning;
            existingVocabulary.Pronunciation = vocabularyDto.Pronunciation;
            existingVocabulary.PartOfSpeech = vocabularyDto.PartOfSpeech;
            existingVocabulary.CEFRLevel = vocabularyDto.CEFRLevel;
            existingVocabulary.Description = vocabularyDto.Description;
            existingVocabulary.ExampleSentence = vocabularyDto.ExampleSentence;
            existingVocabulary.ImageUrl = imageUrl;
            existingVocabulary.PronunciationUrl = vocabularyDto.PronunciationUrl;

            var updatedVocabulary = await _vocabularyRepository.UpdateVocabularyAsync(existingVocabulary);
            if (updatedVocabulary == null)
            {
                _logger.LogWarning("Failed to update vocabulary set with ID: {Id}", id);
                return null;
            }

            _cache.Remove($"Vocabulary_{id}"); // Xóa cache của từ vựng cụ thể
            _logger.LogInformation("Cache invalidated for vocabulary ID: {Id}", id);
            return new AdminVocabularyDto
            {
                Id = updatedVocabulary.Id,
                Word = updatedVocabulary.Word,
                Meaning = updatedVocabulary.Meaning,
                Pronunciation = updatedVocabulary.Pronunciation,
                PartOfSpeech = updatedVocabulary.PartOfSpeech.ToString(),
                CEFRLevel = updatedVocabulary.CEFRLevel.ToString(),
                Description = updatedVocabulary.Description,
                ExampleSentence = updatedVocabulary.ExampleSentence,
                ImageUrl = updatedVocabulary.ImageUrl,
                PronunciationUrl = updatedVocabulary.PronunciationUrl
            };
        }


        //------------------------------ DELETE -----------------------------------------
        // Xóa từ vựng theo ID
        public async Task<bool> DeleteVocabularyAsync(int id)
        {
            var result = await _vocabularyRepository.DeleteVocabularyAsync(id);
            if (result)
            {
                _cache.Remove($"Vocabulary_{id}");
                _logger.LogInformation("Cache invalidated after deleting vocabulary ID: {Id}", id);
            }
            return result;
        }




    }
}