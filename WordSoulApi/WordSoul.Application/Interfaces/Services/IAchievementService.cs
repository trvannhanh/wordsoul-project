using WordSoul.Application.DTOs.Achievement;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IAchievementService
    {
        Task<AchievementDto> CreatAchievementAsync(CreateAchievementDto createAchievementDto);
    }
}