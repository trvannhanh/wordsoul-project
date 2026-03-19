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
        private readonly IBattleService _battleService;
        private readonly ILogger<GymController> _logger;

        public GymController(
            IGymLeaderService gymLeaderService,
            IBattleService battleService,
            ILogger<GymController> logger)
        {
            _gymLeaderService = gymLeaderService;
            _battleService = battleService;
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

        // ══════════════════════════════════════════════════
        // BATTLE ENDPOINTS
        // ══════════════════════════════════════════════════

        // POST: api/gym/{gymId}/battle/start
        // Bắt đầu thách đấu Gym Leader — trả về BattleSession + câu hỏi
        [HttpPost("{gymId:int}/battle/start")]
        public async Task<IActionResult> StartGymBattle(int gymId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _battleService.StartGymBattleAsync(userId, gymId, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Gym locked OR still on cooldown
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting battle for user {UserId}, gym {GymId}", userId, gymId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/gym/battle/{sessionId}/submit
        // Submit kết quả toàn bộ battle
        [HttpPost("battle/{sessionId:int}/submit")]
        public async Task<IActionResult> SubmitBattle(
            int sessionId,
            [FromBody] SubmitBattleRequestDto request,
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            if (request == null || request.Answers.Count == 0)
                return BadRequest(new { message = "No answers provided." });

            try
            {
                var result = await _battleService.SubmitBattleAsync(userId, sessionId, request, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting battle {SessionId} for user {UserId}", sessionId, userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
