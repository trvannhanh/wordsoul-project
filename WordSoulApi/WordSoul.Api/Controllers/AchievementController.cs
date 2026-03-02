using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Api.Extensions;
using WordSoul.Application.DTOs.Achievement;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Enums;
using WordSoul.Domain.Exceptions;

namespace WordSoul.Api.Controllers
{
    [Route("api/achievements")]
    [ApiController]
    public class AchievementController : ControllerBase
    {
        private readonly IAchievementService _achievementService;
        private readonly IUserAchievementService _userAchievementService;
        private readonly ILogger<AchievementController> _logger;

        public AchievementController(
            IAchievementService achievementService,
            IUserAchievementService userAchievementService,
            ILogger<AchievementController> logger)
        {
            _achievementService = achievementService;
            _userAchievementService = userAchievementService;
            _logger = logger;
        }

        // ===================== ADMIN ENDPOINTS =====================

        // POST: api/achievements
        // Tạo achievement template mới (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<AchievementDto>> CreateAchievement(
            [FromBody] CreateAchievementDto createDto,
            CancellationToken ct)
        {
            try
            {
                var achievement = await _achievementService.CreateAchievementAsync(createDto, ct);
                return CreatedAtAction(nameof(GetAchievementById),
                    new { achievementId = achievement.Id }, achievement);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating achievement");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/achievements
        // Lấy danh sách achievement (Admin - có filter và phân trang)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AchievementDto>>> GetAchievements(
            [FromQuery] ConditionType? conditionType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            try
            {
                var achievements = await _achievementService
                    .GetAchievementsAsync(conditionType, pageNumber, pageSize, ct);

                return Ok(achievements);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting achievements");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/achievements/{achievementId}
        // Lấy chi tiết một achievement (Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet("{achievementId}")]
        public async Task<ActionResult<AchievementDto>> GetAchievementById(
            int achievementId,
            CancellationToken ct)
        {
            try
            {
                var achievement = await _achievementService
                    .GetAchievementByIdAsync(achievementId, ct);

                return Ok(achievement);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting achievement {AchievementId}", achievementId);
                return StatusCode(500, "Internal server error");
            }
        }

        // ===================== USER ENDPOINTS =====================

        // GET: api/achievements/me
        // Lấy danh sách achievement của user hiện tại kèm tiến độ
        [Authorize(Roles = "User")]
        [HttpGet("me")]
        public async Task<ActionResult<List<UserAchievementDto>>> GetMyAchievements(
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var achievements = await _userAchievementService
                    .GetUserAchievementsAsync(userId, ct);

                return Ok(achievements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting achievements for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/achievements/{achievementId}/claim
        // Nhận phần thưởng cho achievement đã hoàn thành
        [Authorize(Roles = "User")]
        [HttpPost("{achievementId}/claim")]
        public async Task<IActionResult> ClaimAchievementReward(
            int achievementId,
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                await _userAchievementService
                    .ClaimAchievementRewardAsync(userId, achievementId, ct);

                return Ok(new { Message = "Reward claimed successfully." });
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex,
                    "Achievement {AchievementId} not found for user {UserId}",
                    achievementId, userId);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Chưa hoàn thành hoặc đã claim rồi
                _logger.LogWarning(ex,
                    "Invalid claim for achievement {AchievementId}, user {UserId}",
                    achievementId, userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error claiming achievement {AchievementId} for user {UserId}",
                    achievementId, userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}