using WordSoulApi.Models.DTOs.Achievement;

namespace WordSoulApi.Services.Interfaces
{
    public interface IAchievementService
    {
        Task<AchievementDto> CreatAchievementAsync(CreateAchievementDto createAchievementDto);
    }
}