
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.DailyQuest;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class DailyQuestService : IDailyQuestService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserInventoryService _inventoryService;
        private readonly ILogger<DailyQuestService> _logger;

        public DailyQuestService(IUnitOfWork uow, IUserInventoryService inventoryService,
            ILogger<DailyQuestService> logger)
        {
            _uow = uow;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task GenerateDailyQuestsForUserAsync(
            int userId,
            CancellationToken ct = default)
        {
            // Sử dụng UtcNow để tránh lỗi timezone, chỉ lấy phần Date để so sánh
            var today = DateTime.UtcNow.Date;

            // Check đã generate chưa
            var existing = await _uow.UserDailyQuest
                .GetUserDailyQuestsByUserAndDateAsync(userId, today, ct);

            // Nếu đã tồn tại, không tạo lại để tránh trùng lặp và reset tiến độ
            if (existing.Count != 0)
                return;

            // Lấy tất cả quest template đang active (có thể có thêm logic lọc theo level, loại quest,...)
            var activeTemplates = await _uow.DailyQuest
                .GetActiveQuestsAsync(ct);

            // Tạo UserDailyQuest cho mỗi template và lưu vào database
            foreach (var template in activeTemplates)
            {
                var userQuest = new UserDailyQuest
                {
                    UserId = userId,
                    DailyQuestId = template.Id,
                    Progress = 0,
                    IsCompleted = false,
                    IsClaimed = false,
                    QuestDate = today
                };

                await _uow.UserDailyQuest
                    .CreateUserDailyQuestAsync(userQuest, ct);
            }

            await _uow.SaveChangesAsync(ct);
        }

        public async Task<List<UserDailyQuest>> GetUserDailyQuestsAsync(
            int userId,
            DateTime date,
            CancellationToken ct = default)
        {
            return await _uow.UserDailyQuest
                .GetUserDailyQuestsByUserAndDateAsync(userId, date.Date, ct);
        }

        //  Lấy danh sách quest template đang active
        public async Task<List<DailyQuestDto>> GetActiveQuestsAsync(
            CancellationToken ct = default)
        {
            var quests = await _uow.DailyQuest.GetActiveQuestsAsync(ct);

            return quests.Select(q => new DailyQuestDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                QuestType = q.QuestType.ToString(),
                TargetValue = q.TargetValue,
                RewardType = q.RewardType.ToString(),
                RewardValue = q.RewardValue,
                RewardReferenceId = q.RewardReferenceId,
                IsActive = q.IsActive,
                CreatedAt = q.CreatedAt
            }).ToList();
        }

        // Tạo quest template mới
        public async Task<DailyQuestDto> CreateQuestAsync(
            CreateDailyQuestDto dto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.", nameof(dto.Title));

            if (dto.TargetValue <= 0)
                throw new ArgumentException("TargetValue must be greater than zero.", nameof(dto.TargetValue));

            if (dto.RewardValue <= 0)
                throw new ArgumentException("RewardValue must be greater than zero.", nameof(dto.RewardValue));

            // Validate RewardReferenceId khi RewardType là Item hoặc Pet
            if ((dto.RewardType == RewardType.Item || dto.RewardType == RewardType.Pet)
                && dto.RewardReferenceId == null)
            {
                throw new ArgumentException(
                    "RewardReferenceId is required when RewardType is Item or Pet.",
                    nameof(dto.RewardReferenceId));
            }

            var quest = new DailyQuest
            {
                Title = dto.Title,
                Description = dto.Description,
                QuestType = dto.QuestType,
                TargetValue = dto.TargetValue,
                RewardType = dto.RewardType,
                RewardValue = dto.RewardValue,
                RewardReferenceId = dto.RewardReferenceId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.DailyQuest.CreateQuestAsync(quest, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Created new DailyQuest: {Title}", quest.Title);

            return new DailyQuestDto
            {
                Id = quest.Id,
                Title = quest.Title,
                Description = quest.Description,
                QuestType = quest.QuestType.ToString(),
                TargetValue = quest.TargetValue,
                RewardType = quest.RewardType.ToString(),
                RewardValue = quest.RewardValue,
                RewardReferenceId = quest.RewardReferenceId,
                IsActive = quest.IsActive,
                CreatedAt = quest.CreatedAt
            };
        }

        // 3. Bật/tắt trạng thái active của quest template
        public async Task ToggleQuestActiveAsync(
            int questId,
            CancellationToken ct = default)
        {
            var quest = await _uow.DailyQuest.GetByIdAsync(questId, ct)
                ?? throw new KeyNotFoundException($"DailyQuest with ID {questId} not found.");

            quest.IsActive = !quest.IsActive;

            await _uow.DailyQuest.UpdateQuestAsync(quest, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "DailyQuest {QuestId} is now {Status}",
                questId, quest.IsActive ? "active" : "inactive");
        }

        // 4. Nhận phần thưởng cho quest đã hoàn thành
        public async Task<ClaimQuestRewardResponseDto> ClaimRewardAsync(
            int userId,
            int userDailyQuestId,
            CancellationToken ct = default)
        {
            // Lấy UserDailyQuest và kiểm tra ownership
            var userQuest = await _uow.UserDailyQuest.GetByIdAsync(userDailyQuestId, ct)
                ?? throw new KeyNotFoundException($"UserDailyQuest with ID {userDailyQuestId} not found.");

            if (userQuest.UserId != userId)
                throw new UnauthorizedAccessException("You do not own this quest.");

            if (!userQuest.IsCompleted)
                throw new InvalidOperationException("Quest is not completed yet.");

            if (userQuest.IsClaimed)
                throw new InvalidOperationException("Reward has already been claimed.");

            var quest = userQuest.DailyQuest
                ?? throw new InvalidOperationException("Quest template data is missing.");

            // Xử lý từng loại reward
            await using var transaction = await _uow.BeginTransactionAsync(ct);
            try
            {
                string rewardMessage = await ProcessRewardAsync(userId, quest, ct);

                // Đánh dấu đã claim
                userQuest.IsClaimed = true;
                await _uow.UserDailyQuest.UpdateUserDailyQuestAsync(userQuest, ct);
                await _uow.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation(
                    "User {UserId} claimed reward for quest {QuestId}",
                    userId, quest.Id);

                return new ClaimQuestRewardResponseDto
                {
                    RewardType = quest.RewardType.ToString(),
                    RewardValue = quest.RewardValue,
                    RewardReferenceId = quest.RewardReferenceId,
                    Message = rewardMessage
                };
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        // Helper: xử lý phần thưởng theo RewardType
        private async Task<string> ProcessRewardAsync(
            int userId,
            DailyQuest quest,
            CancellationToken ct)
        {
            switch (quest.RewardType)
            {
                case RewardType.XP:
                    await _uow.User.UpdateUserXPAndAPAsync(userId, quest.RewardValue, 0, ct);
                    return $"You earned {quest.RewardValue} XP!";

                case RewardType.AP:
                    await _uow.User.UpdateUserXPAndAPAsync(userId, 0, quest.RewardValue, ct);
                    return $"You earned {quest.RewardValue} AP!";

                case RewardType.Item:
                    if (quest.RewardReferenceId == null)
                        throw new InvalidOperationException("Reward item ID is missing.");

                    await _inventoryService.AddItemToUserAsync(
                        userId, quest.RewardReferenceId.Value, quest.RewardValue, ct);

                    return $"You received {quest.RewardValue} item(s)!";

                default:
                    _logger.LogWarning(
                        "Unknown RewardType {RewardType} for quest {QuestId}",
                        quest.RewardType, quest.Id);
                    throw new InvalidOperationException($"Unsupported reward type: {quest.RewardType}");
            }
        }

        public async Task UpdateQuestProgressAsync(
            int userId,
            QuestType questType,
            int increment = 1,
            double? accuracy = null,
            CancellationToken ct = default)
        {

            await using var transaction = await _uow.BeginTransactionAsync(ct);
            try
            {
                var today = DateTime.UtcNow.Date;
                var quests = await _uow.UserDailyQuest
                    .GetUserDailyQuestsByUserAndDateAsync(userId, today, ct);

                var targetQuest = quests
                    .FirstOrDefault(q => q.DailyQuest!.QuestType == questType);

                if (targetQuest == null || targetQuest.IsCompleted)
                {
                    await transaction.CommitAsync(ct);
                    return;
                }

                switch (questType)
                {
                    case QuestType.Learn:
                    case QuestType.Review:
                    case QuestType.Catch:
                        targetQuest.Progress += increment;
                        break;

                    case QuestType.Accuracy:
                        if (accuracy.HasValue && accuracy.Value >= 0.8)
                            targetQuest.Progress += 1;
                        break;
                }

                if (targetQuest.Progress >= targetQuest.DailyQuest!.TargetValue)
                {
                    targetQuest.Progress = targetQuest.DailyQuest.TargetValue;
                    targetQuest.IsCompleted = true;
                }

                await _uow.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

    }
}
