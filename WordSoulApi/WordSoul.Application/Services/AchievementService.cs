using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Achievement;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.Domain.Exceptions;

namespace WordSoul.Application.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AchievementService> _logger;

        public AchievementService(IUnitOfWork uow, ILogger<AchievementService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        //---------------------CREATE-------------------
        public async Task<AchievementDto> CreateAchievementAsync(
            CreateAchievementDto createAchievementDto,
            CancellationToken ct = default)
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

                await _uow.Achievement.CreateAchievementAsync(achievement, ct);

                await _uow.SaveChangesAsync(ct);

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
        public async Task<List<AchievementDto>> GetAchievementsAsync(
            ConditionType? conditionType,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0.");

            var achievements = await _uow.Achievement
                .GetAchievementsAsync(conditionType, pageNumber, pageSize, ct);

            return achievements.Select(a => new AchievementDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                ConditionType = a.ConditionType.ToString(),
                ConditionValue = a.ConditionValue,
                ItemName = a.Item?.Name,
                ItemImageUrl = a.Item?.ImageUrl,
            }).ToList();
        }

        public async Task<AchievementDto> GetAchievementByIdAsync(
            int achievementId,
            CancellationToken ct = default)
        {
            var achievement = await _uow.Achievement.GetAchievementByIdAsync(achievementId, ct);

            if (achievement == null)
            {
                _logger.LogWarning("Achievement with ID: {AchievementId} not found", achievementId);
                throw new NotFoundException(nameof(Achievement), achievementId);
            }

            return new AchievementDto
            {
                Id = achievement.Id,
                Name = achievement.Name,
                Description = achievement.Description,
                ConditionType = achievement.ConditionType.ToString(),
                ConditionValue = achievement.ConditionValue,
                ItemName = achievement.Item?.Name,
                ItemImageUrl = achievement.Item?.ImageUrl,
            };
        }
    }
}
