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

        [HttpPut("quests/{id}")]
        public async Task<IActionResult> UpdateQuest(int id, [FromBody] UpdateDailyQuestDto dto)
        {
            var result = await _questService.UpdateQuestAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
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

        [HttpPut("achievements/{id}")]
        public async Task<IActionResult> UpdateAchievement(int id, [FromBody] UpdateAchievementDto dto)
        {
            var result = await _achievementService.UpdateAchievementAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("quests/{id}")]
        public async Task<IActionResult> DeleteQuest(int id)
        {
            var deleted = await _questService.DeleteQuestAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpDelete("achievements/{id}")]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var deleted = await _achievementService.DeleteAchievementAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
