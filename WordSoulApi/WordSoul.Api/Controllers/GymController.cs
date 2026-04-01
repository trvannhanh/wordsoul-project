using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Api.Extensions;
using WordSoul.Application.DTOs.Gym;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Controllers
{
    [Route("api/gym")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class GymController : ControllerBase
    {
        private readonly IGymLeaderService _gymLeaderService;
        private readonly ILogger<GymController> _logger;

        public GymController(
            IGymLeaderService gymLeaderService,
            ILogger<GymController> logger)
        {
            _gymLeaderService = gymLeaderService;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════
        // GYM INFO ENDPOINTS
        // ══════════════════════════════════════════════════

        // GET: api/gym
        // Lấy danh sách 8 Gym Leader kèm trạng thái tiến trình của user hiện tại
        [HttpGet]
        public async Task<IActionResult> GetAllGyms(CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var gyms = await _gymLeaderService.GetAllGymsForUserAsync(userId, ct);
                return Ok(gyms);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching gym list for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/gym/{gymId}
        // Lấy chi tiết một Gym Leader (kèm điều kiện mở khóa và cooldown)
        [HttpGet("{gymId:int}")]
        public async Task<IActionResult> GetGymDetail(int gymId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var gym = await _gymLeaderService.GetGymDetailAsync(userId, gymId, ct);
                if (gym == null) return NotFound($"Gym {gymId} not found.");
                return Ok(gym);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching gym {GymId} for user {UserId}", gymId, userId);
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
