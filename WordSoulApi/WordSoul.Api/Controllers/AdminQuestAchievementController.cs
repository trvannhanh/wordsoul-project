using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.DTOs.DailyQuest;
using WordSoul.Application.DTOs.Achievement;

namespace WordSoul.Api.Controllers
{
    [Route("api/admin/quests-achievements")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminQuestAchievementController : ControllerBase
    {
        private readonly IDailyQuestService _questService;
        private readonly IAchievementService _achievementService;

        public AdminQuestAchievementController(IDailyQuestService questService, IAchievementService achievementService)
        {
            _questService = questService;
            _achievementService = achievementService;
        }

        // --- Quests ---

        [HttpGet("quests")]
        public async Task<IActionResult> GetAllQuests()
        {
            var quests = await _questService.GetActiveQuestsAsync();
            return Ok(quests);
        }

        [HttpPost("quests")]
        public async Task<IActionResult> CreateQuest([FromBody] CreateDailyQuestDto dto)
        {
            var result = await _questService.CreateQuestAsync(dto);
            return Ok(result);
        }

        [HttpPatch("quests/{id}/toggle")]
        public async Task<IActionResult> ToggleQuest(int id)
        {
            await _questService.ToggleQuestActiveAsync(id);
            return NoContent();
        }

        // --- Achievements ---

        [HttpGet("achievements")]
        public async Task<IActionResult> GetAllAchievements([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var achievements = await _achievementService.GetAchievementsAsync(null, page, size);
            return Ok(achievements);
        }

        [HttpPost("achievements")]
        public async Task<IActionResult> CreateAchievement([FromBody] CreateAchievementDto dto)
        {
            var result = await _achievementService.CreateAchievementAsync(dto);
            return Ok(result);
        }
    }
}
