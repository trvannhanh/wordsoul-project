using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.DTOs;
using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class VocabularyService : IVocabularyService
    {   
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly ILogger<VocabularySetService> _logger;
        private readonly IVocabularySetRepository _vocabularySetRepository;

        public VocabularyService(IVocabularyRepository vocabularyRepository, ILogger<VocabularySetService> logger, IVocabularySetRepository vocabularySetRepository)
        {
            _vocabularyRepository = vocabularyRepository;
            _logger = logger;
            _vocabularySetRepository = vocabularySetRepository;
        }

        // Lấy tất cả các từ vựng
        public async Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber, int pageSize)
        {
            var vocabularies = await _vocabularyRepository.GetAllVocabulariesAsync(word, meaning, partOfSpeech, cEFRLevel, pageNumber, pageSize);
            var vocabularyDtos = new List<VocabularyDto>();
            foreach (var vocabulary in vocabularies)
            {
                vocabularyDtos.Add(new VocabularyDto
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
                });
            }
            return vocabularyDtos;
        }

        // Lấy từ vựng theo ID
        public async Task<VocabularyDto?> GetVocabularyByIdAsync(int id)
        {
            var vocabulary = await _vocabularyRepository.GetVocabularyByIdAsync(id);
            if (vocabulary == null) return null;
            return new VocabularyDto
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
        }


        // Tạo từ vựng mới
        public async Task<AdminVocabularyDto> CreateVocabularyAsync(CreateVocabularyDto vocabularyDto, string? imageUrl)
        {
            _logger.LogInformation("Creating vocabulary with word: {Word}", vocabularyDto.Word);

            if (string.IsNullOrWhiteSpace(vocabularyDto.Word))
            {
                _logger.LogError("Word is required for creating a vocabulary ");
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

            var createdVocabulary = await _vocabularyRepository.CreateVocabularyAsync(vocabulary);
            _logger.LogInformation("Vocabulary set created with ID: {Id}", createdVocabulary.Id);

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

            _logger.LogInformation("Vocabulary updated with ID: {Id}", updatedVocabulary.Id);
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

        // Xóa từ vựng theo ID
        public async Task<bool> DeleteVocabularyAsync(int id)
        {
            return await _vocabularyRepository.DeleteVocabularyAsync(id);
        }

        // Lấy các từ vựng theo danh sách từ
        public async Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(SearchVocabularyDto searchVocabularyDto)
        {
            if (searchVocabularyDto == null || searchVocabularyDto.Words == null || !searchVocabularyDto.Words.Any())
                return new List<VocabularyDto>();


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
                PronunciationUrl = v.PronunciationUrl
            }).ToList();

            return vocabularyDtos;
        }

        // Thêm từ vựng mới vào bộ từ vựng
        public async Task<AdminVocabularyDto?> AddVocabularyToSetAsync(int setId, CreateVocabularyInSetDto vocabularyDto, string? imageUrl)
        {
            // Kiểm tra bộ từ vựng tồn tại 
            var vocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(setId);
            if (vocabularySet == null)
                throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");

            // Kiểm tra từ vựng đã tồn tại trong bộ chưa
            var existingLink = await _vocabularyRepository.CheckVocabularyExistFromSessionAsync(vocabularyDto.Word, setId);
            if (existingLink)
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

            await _vocabularyRepository.CreateVocabularyAsync(vocabulary);

            // Lấy max Order và gán +1
            var maxOrderValue = await _vocabularyRepository.GetVocabularyOrderMaxAsync(setId);
            var newOrder = maxOrderValue + 1;

            // Tạo liên kết many-to-many
            var setVocabulary = new SetVocabulary
            {
                VocabularySetId = setId,
                VocabularyId = vocabulary.Id,
                Order = newOrder  
            };

            await _vocabularySetRepository.CreateSetVocabularyAsync(setVocabulary);

            // Map sang DTO
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

        // Xóa liên kết từ vựng khỏi bộ
        public async Task<bool> RemoveVocabularyFromSetAsync(int setId, int vocabId)
        {
            var setVocabulary = await _vocabularySetRepository.GetSetVocabularyAsync(vocabId, setId);
            if (setVocabulary == null) return false;

            return await _vocabularySetRepository.DeleteSetVocabularyAsync(setVocabulary);
        }

        // Lấy danh sách từ vựng trong bộ với phân trang
        public async Task<PagedResult<VocabularyDto>> GetVocabulariesInSetAsync(int setId, int pageNumber = 1, int pageSize = 10)
        {
            var vocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(setId);
            if (vocabularySet == null)
                throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");

            var (vocabularies, totalCount) = await _vocabularyRepository.GetVocabulariesFromSetAsync(setId, pageNumber, pageSize);

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

            return new PagedResult<VocabularyDto>
            {
                Items = vocabularyDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }
    }
}