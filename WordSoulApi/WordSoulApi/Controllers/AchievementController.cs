
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Application.DTOs.Achievement;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Controllers
{
    [Route("api/achievement")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    public class AchievementController : ControllerBase
    {
        private readonly IAchievementService _achievementService;
        public AchievementController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
        }


        // POST: api/achievement : Tạo thành tựu mới
        [HttpPost]
        public async Task<ActionResult<CreateAchievementDto>> CreateAchievement(CreateAchievementDto createAchievementDto)
        {
            var achievement = await _achievementService.CreatAchievementAsync(createAchievementDto);
            if (achievement == null)
            {
                return BadRequest("User creating achievement failed.");
            }
            return Ok(achievement);
        }

        

    }
}
