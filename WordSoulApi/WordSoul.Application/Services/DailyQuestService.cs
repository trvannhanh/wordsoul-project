
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class DailyQuestService : IDailyQuestService
    {
        private readonly IUnitOfWork _uow;
        private static readonly Dictionary<int, SemaphoreSlim> _userLocks = new();

        public DailyQuestService(IUnitOfWork uow)
        {
            _uow = uow;
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

        public async Task UpdateQuestProgressAsync(
            int userId,
            QuestType questType,
            int increment = 1,
            double? accuracy = null,
            CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            var userLock = GetLock(userId);

            await userLock.WaitAsync(ct);
            try
            {
                var quests = await _uow.UserDailyQuest
                    .GetUserDailyQuestsByUserAndDateAsync(userId, today, ct);

                var targetQuest = quests
                    .FirstOrDefault(q => q.DailyQuest!.QuestType == questType);

                if (targetQuest == null || targetQuest.IsCompleted)
                    return;

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

                await _uow.UserDailyQuest.UpdateUserDailyQuestAsync(targetQuest, ct);
                await _uow.SaveChangesAsync(ct);
            }
            finally
            {
                userLock.Release();
            }
        }

        private SemaphoreSlim GetLock(int userId)
        {
            lock (_userLocks)
            {
                if (!_userLocks.ContainsKey(userId))
                    _userLocks[userId] = new SemaphoreSlim(1, 1);

                return _userLocks[userId];
            }
        }
    }
}
