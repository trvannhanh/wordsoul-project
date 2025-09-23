using WordSoulApi.Exceptions;
using WordSoulApi.Models.DTOs.Achievement;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class AchievementService : IAchievementService
    {
        private readonly IAchievementRepository _achievementRepository;
        private readonly ILogger<AchievementService> _logger;

        public AchievementService(IAchievementRepository achievementRepository, ILogger<AchievementService> logger)
        {
            _achievementRepository = achievementRepository;
            _logger = logger;
        }

        //---------------------CREATE-------------------
        public async Task<AchievementDto> CreatAchievementAsync(CreateAchievementDto createAchievementDto)
        {
            if (string.IsNullOrWhiteSpace(createAchievementDto.Name))
            {
                _logger.LogError("Achievement Name is required for creating Achievement");
                throw new ArgumentException("Name is required.", nameof(createAchievementDto.Name));
            }

            try
            {
                _logger.LogInformation("Creating Achievement {AchievementName}", createAchievementDto.Name);

                var achievement = new Achievement
                {
                    Name = createAchievementDto.Name,
                    Description = createAchievementDto.Description,
                    ConditionType = createAchievementDto.ConditionType,
                    ConditionValue = createAchievementDto.ConditionValue,
                    RewardItemId = createAchievementDto.ItemId
                };

                await _achievementRepository.CreateAchievementAsync(achievement);
                _logger.LogInformation("Successfully created achievement {AchievementName}", achievement.Name);

                return new AchievementDto
                {
                    Id = achievement.Id,
                    Name = achievement.Name,
                    Description = achievement.Description,
                    ConditionType = achievement.ConditionType.ToString(),
                    ConditionValue = achievement.ConditionValue,
                    ItemImageUrl = achievement?.Item?.ImageUrl,
                    ItemName = achievement?.Item?.Name,
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating achievement");
                throw new Exception($"Error creating achievement : {ex.Message}", ex);
            }
        }

        //------------------------------READ------------------------------
        public async Task<List<AchievementDto>> GetAchievemenstAsync(ConditionType? conditionType , int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");

            var achievements = await _achievementRepository.GetAchievementsAsync(conditionType, pageNumber, pageSize);

            var achiementDtos = achievements.
                Select(a => new AchievementDto {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    ConditionType = a.ConditionType.ToString(),
                    ConditionValue = a.ConditionValue,
                    ItemName = a.Item?.Name,
                    ItemImageUrl = a.Item?.ImageUrl,
                }).ToList();

            return achiementDtos;
        }

        public async Task<AchievementDto> GetAchievementByIdAsync(int achievementId)
        {
            var achievement = await _achievementRepository.GetAchievementByIdAsync(achievementId);
            if (achievement == null)
            {
                _logger.LogWarning("Achievement with ID: {AchievementId} not found", achievementId);
                throw new NotFoundException("Achievement", achievement.Id);
            }

            var achievementDto = new AchievementDto
            {
                Id = achievement.Id,
                Name = achievement.Name,
                Description = achievement.Description,
                ConditionType = achievement.ConditionType.ToString(),
                ConditionValue = achievement.ConditionValue,
                ItemName = achievement.Item?.Name,
                ItemImageUrl = achievement.Item?.ImageUrl,
            };

            return achievementDto;

        }
    }
}
