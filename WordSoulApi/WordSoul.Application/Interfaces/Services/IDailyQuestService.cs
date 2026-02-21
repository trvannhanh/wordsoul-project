
using WordSoul.Application.DTOs.DailyQuest;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IDailyQuestService
    {
        Task GenerateDailyQuestsForUserAsync(
            int userId,
            CancellationToken ct = default);

        Task<List<UserDailyQuest>> GetUserDailyQuestsAsync(
            int userId,
            DateTime date,
            CancellationToken ct = default);

        Task UpdateQuestProgressAsync(
            int userId,
            QuestType questType,
            int increment = 1,
            double? accuracy = null,
            CancellationToken ct = default);

        Task<ClaimQuestRewardResponseDto> ClaimRewardAsync(int userId, int userDailyQuestId, CancellationToken ct = default);
        Task<List<DailyQuestDto>> GetActiveQuestsAsync(CancellationToken ct = default);
        Task<DailyQuestDto> CreateQuestAsync(CreateDailyQuestDto dto, CancellationToken ct = default);
        Task ToggleQuestActiveAsync(int questId, CancellationToken ct = default);
    }
}
