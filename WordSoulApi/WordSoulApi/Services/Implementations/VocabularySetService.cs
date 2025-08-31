using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<VocabularySetService> _logger;

        public VocabularySetService(
            IVocabularySetRepository vocabularySetRepository,
            IVocabularyRepository vocabularyRepository,
            ILogger<VocabularySetService> logger)
        {
            _vocabularySetRepository = vocabularySetRepository;
            _vocabularyRepository = vocabularyRepository;
            _logger = logger;
        }

        // Lấy tất cả các bộ từ vựng với phân trang
        public async Task<IEnumerable<VocabularySetDto>> GetAllVocabularySetsAsync(int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("Retrieving all vocabulary sets with pageNumber: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);
            var vocabularySets = await _vocabularySetRepository.GetAllVocabularySetsAsync(pageNumber, pageSize);
            var vocabularySetDtos = new List<VocabularySetDto>();

            foreach (var vocabularySet in vocabularySets)
            {
                vocabularySetDtos.Add(MapToDto(vocabularySet));
            }

            return vocabularySetDtos;
        }

        // Lấy bộ từ vựng theo ID
        public async Task<VocabularySetDetailDto?> GetVocabularySetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving vocabulary set with ID: {Id}", id);
            var vocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(id);
            if (vocabularySet == null)
            {
                _logger.LogWarning("Vocabulary set with ID: {Id} not found", id);
                return null;
            }

            return MapToDetailDto(vocabularySet);
        }

        // Lấy bộ từ vựng theo ID kèm chi tiết các từ vựng bên trong với phân trang
        public async Task<VocabularySetFullDetailDto?> GetVocabularySetFullDetailsAsync(int id, int page, int pageSize)
        {
            _logger.LogInformation("Retrieving full details for vocabulary set with ID: {Id}, Page: {Page}, PageSize: {PageSize}", id, page, pageSize);
            var vocabularySet = await _vocabularySetRepository.GetVocabularySetFullDetailsAsync(id, page, pageSize); // Use the detailed repository method
            if (vocabularySet == null)
            {
                _logger.LogWarning("Vocabulary set with ID: {Id} not found", id);
                return null;
            }

            return MapToFullDetailDto(vocabularySet, page, pageSize); // Maps to VocabularySetFullDetailDto with full vocabularies
        }

        // Tạo bộ từ vựng mới
        public async Task<VocabularySetDto> CreateVocabularySetAsync(CreateVocabularySetDto createDto, string? imageUrl)
        {
            _logger.LogInformation("Creating vocabulary set with title: {Title}", createDto.Title);
                
            if (string.IsNullOrWhiteSpace(createDto.Title))
            {
                _logger.LogError("Title is required for creating a vocabulary set");
                throw new ArgumentException("Title is required.", nameof(createDto.Title));
            }

            // Validate VocabularyIds
            foreach (var vocabId in createDto.VocabularyIds)
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
                SetVocabularies = createDto.VocabularyIds.Select((vocabId, index) => new SetVocabulary
                {
                    VocabularyId = vocabId,
                    Order = index + 1
                }).ToList()
            };

            var createdVocabularySet = await _vocabularySetRepository.CreateVocabularySetAsync(vocabularySet);
            _logger.LogInformation("Vocabulary set created with ID: {Id}", createdVocabularySet.Id);

            return MapToDto(createdVocabularySet);
        }

        // Cập nhật bộ từ vựng
        public async Task<VocabularySetDto?> UpdateVocabularySetAsync(int id, UpdateVocabularySetDto updateDto)
        {
            _logger.LogInformation("Updating vocabulary set with ID: {Id}", id);

            if (string.IsNullOrWhiteSpace(updateDto.Title))
            {
                _logger.LogError("Title is required for updating a vocabulary set");
                throw new ArgumentException("Title is required.", nameof(updateDto.Title));
            }

            // Validate VocabularyIds
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

            _logger.LogInformation("Vocabulary set updated with ID: {Id}", updatedVocabularySet.Id);
            return MapToDto(updatedVocabularySet);
        }

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
                _logger.LogInformation("Vocabulary set with ID: {Id} deleted", id);
            }
            return result;
        }


        private VocabularySetDto MapToDto(VocabularySet vocabularySet)
        {
            return new VocabularySetDto
            {
                Id = vocabularySet.Id,
                Title = vocabularySet.Title,
                Theme = vocabularySet.Theme.ToString(),
                ImageUrl = vocabularySet.ImageUrl,
                Description = vocabularySet.Description,
                DifficultyLevel = vocabularySet.DifficultyLevel.ToString(),
                IsActive = vocabularySet.IsActive,
                CreatedAt = vocabularySet.CreatedAt,
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

        private VocabularySetFullDetailDto MapToFullDetailDto(VocabularySet vocabularySet, int page, int pageSize)
        {
            var totalVocabularies = _vocabularySetRepository.CountVocabulariesInSetAsync(vocabularySet.Id).Result; // Count total for pagination metadata
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

        // Tìm kiếm bộ từ vựng với các tiêu chí khác nhau và phân trang
        public async Task<IEnumerable<VocabularySetDto>> SearchVocabularySetAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty,
                                                                        DateTime? createdAfter, int pageNumber, int pageSize)
        {
            var sets = await _vocabularySetRepository.SearchVocabularySetAsync(title, theme, difficulty, createdAfter, pageNumber, pageSize);
            var vocabularySetDtos = new List<VocabularySetDto>();
            foreach (var vocabularySet in sets)
            {
                vocabularySetDtos.Add(MapToDto(vocabularySet));
            }

            return vocabularySetDtos;


            //return sets.Select(vs => new VocabularySetDto
            //{
            //    Id = vs.Id,
            //    Title = vs.Title,
            //    Theme = vs.Theme.ToString(),
            //    DifficultyLevel = vs.DifficultyLevel.ToString(),
            //    CreatedAt = vs.CreatedAt
            //});
        }

    }
}
