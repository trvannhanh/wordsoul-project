using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Api.Extensions;
using WordSoul.Application.DTOs.DailyQuest;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Controllers
{
    [Route("api/daily-quests")]
    [ApiController]
    public class DailyQuestController : ControllerBase
    {
        private readonly IDailyQuestService _dailyQuestService;
        private readonly ILogger<DailyQuestController> _logger;

        public DailyQuestController(
            IDailyQuestService dailyQuestService,
            ILogger<DailyQuestController> logger)
        {
            _dailyQuestService = dailyQuestService;
            _logger = logger;
        }

        // GET: api/daily-quests/today
        // Lấy danh sách quest của user trong ngày hôm nay
        [Authorize(Roles = "User")]
        [HttpGet("today")]
        public async Task<ActionResult<List<UserDailyQuestDto>>> GetTodayQuests(
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var quests = await _dailyQuestService
                    .GetUserDailyQuestsAsync(userId, DateTime.UtcNow, ct);

                // Map entity sang DTO để không expose entity trực tiếp
                var result = quests.Select(q => new UserDailyQuestDto
                {
                    Id = q.Id,
                    DailyQuestId = q.DailyQuestId,
                    Title = q.DailyQuest!.Title,
                    Description = q.DailyQuest.Description,
                    QuestType = q.DailyQuest.QuestType.ToString(),
                    Progress = q.Progress,
                    TargetValue = q.DailyQuest.TargetValue,
                    RewardType = q.DailyQuest.RewardType.ToString(),
                    RewardValue = q.DailyQuest.RewardValue,
                    IsCompleted = q.IsCompleted,
                    IsClaimed = q.IsClaimed,
                    QuestDate = q.QuestDate
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today quests for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/daily-quests/{userDailyQuestId}/claim
        // Nhận phần thưởng cho quest đã hoàn thành
        [Authorize(Roles = "User")]
        [HttpPost("{userDailyQuestId}/claim")]
        public async Task<ActionResult<ClaimQuestRewardResponseDto>> ClaimReward(
            int userDailyQuestId,
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var response = await _dailyQuestService.ClaimRewardAsync(userId, userDailyQuestId, ct);

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex,
                    "Quest {QuestId} not found for user {UserId}",
                    userDailyQuestId, userId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex,
                    "User {UserId} tried to claim quest {QuestId} they don't own",
                    userId, userDailyQuestId);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                // Quest chưa hoàn thành hoặc đã claim rồi
                _logger.LogWarning(ex,
                    "Invalid claim attempt for quest {QuestId}, user {UserId}",
                    userDailyQuestId, userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error claiming reward for quest {QuestId}, user {UserId}",
                    userDailyQuestId, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        // ---- Admin endpoints ----

        // GET: api/daily-quests
        // Lấy tất cả quest template đang active (Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<DailyQuestDto>>> GetActiveQuests(
            CancellationToken ct)
        {
            try
            {
                var quests = await _dailyQuestService.GetActiveQuestsAsync(ct);
                return Ok(quests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active quests");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/daily-quests
        // Tạo quest template mới (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<DailyQuestDto>> CreateQuest(
            [FromBody] CreateDailyQuestDto createDto,
            CancellationToken ct)
        {
            try
            {
                var quest = await _dailyQuestService.CreateQuestAsync(createDto, ct);
                return CreatedAtAction(nameof(GetActiveQuests), quest);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily quest");
                return StatusCode(500, "Internal server error");
            }
        }

        // PATCH: api/daily-quests/{questId}/toggle-active
        // Bật/tắt quest template (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPatch("{questId}/toggle-active")]
        public async Task<IActionResult> ToggleQuestActive(int questId, CancellationToken ct)
        {
            try
            {
                await _dailyQuestService.ToggleQuestActiveAsync(questId, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling quest {QuestId}", questId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}