
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class DailyQuestService : IDailyQuestService
    {
        private readonly IUnitOfWork _uow;

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
    }
}
