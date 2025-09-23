using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Models.DTOs.Achievement;
using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
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
