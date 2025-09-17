using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
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

        public VocabularySetService(
            IVocabularySetRepository vocabularySetRepository,
            IVocabularyRepository vocabularyRepository,
            ILogger<VocabularySetService> logger,
            IUserVocabularySetRepository userVocabularySetRepository,
            IUserRepository userRepository,
            IPetRepository petRepository)
        {
            _vocabularySetRepository = vocabularySetRepository;
            _vocabularyRepository = vocabularyRepository;
            _logger = logger;
            _userVocabularySetRepository = userVocabularySetRepository;
            _userRepository = userRepository;
            _petRepository = petRepository;
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
        public async Task<VocabularySetDto> CreateVocabularySetAsync(CreateVocabularySetDto createDto, string? imageUrl, int userId)
        {
            _logger.LogInformation("Creating vocabulary set with title: {Title} by user ID: {UserId}", createDto.Title, userId);

            // Kiểm tra user tồn tại
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User with ID: {UserId} not found", userId);
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Kiểm tra title hợp lệ
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

            // Tạo VocabularySet mới
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
                SetVocabularies = createDto.VocabularyIds.Select((vocabId, index) => new SetVocabulary
                {
                    VocabularyId = vocabId,
                    Order = index + 1
                }).ToList(),
                SetRewardPets = new List<SetRewardPet>()
            };

            // Tự động gán Pet theo Rarity
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
                    _logger.LogWarning("Not enough pets with rarity {Rarity}. Requested: {Count}, Found: {Found}", rarity, count, pets.Count);
                    // Tiếp tục với số lượng có sẵn hoặc throw exception tùy yêu cầu
                    // throw new InvalidOperationException($"Not enough pets with rarity {rarity}. Required: {count}, Found: {pets.Count}");
                }

                vocabularySet.SetRewardPets.AddRange(pets.Select(pet => new SetRewardPet
                {
                    PetId = pet.Id,
                    DropRate = dropRate
                }));
            }

            // Lưu VocabularySet (bao gồm SetVocabularies và SetRewardPets)
            var createdVocabularySet = await _vocabularySetRepository.CreateVocabularySetAsync(vocabularySet);

            // Tự động gán quyền sở hữu cho người tạo qua UserVocabularySet
            var userVocabularySet = new UserVocabularySet
            {
                UserId = userId,
                VocabularySetId = createdVocabularySet.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _userVocabularySetRepository.AddVocabularySetToUserAsync(userVocabularySet);

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
        public async Task<IEnumerable<VocabularySetDto>> GetAllVocabularySetsAsync(
            string? title,
            VocabularySetTheme? theme,
            VocabularyDifficultyLevel? difficulty,
            DateTime? createdAfter,
            bool? isOwned,
            int? userId, // Mới: Nullable để hỗ trợ chưa đăng nhập
            int pageNumber,
            int pageSize)
        {
            var sets = await _vocabularySetRepository.GetAllVocabularySetsAsync(
                title, theme, difficulty, createdAfter, isOwned, userId, pageNumber, pageSize);

            var vocabularySetDtos = new List<VocabularySetDto>();
            foreach (var vocabularySet in sets)
            {
                vocabularySetDtos.Add(MapToDto(vocabularySet, userId));
            }

            return vocabularySetDtos;
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

    }
}
