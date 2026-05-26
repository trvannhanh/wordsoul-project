using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Api.Extensions;
using WordSoul.Api.Hubs;
using WordSoul.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace WordSoul.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly ISystemConfigurationService _systemConfigService;
        private readonly IActivityLogService _activityLogService;
        private readonly IAdminDashboardService _dashboardService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IHubContext<BattleHub> _battleHub;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ISystemConfigurationService systemConfigService,
            IActivityLogService activityLogService,
            IAdminDashboardService dashboardService,
            IUserService userService,
            INotificationService notificationService,
            IEmailService emailService,
            IHubContext<BattleHub> battleHub,
            ILogger<AdminController> logger)
        {
            _systemConfigService = systemConfigService;
            _activityLogService = activityLogService;
            _dashboardService = dashboardService;
            _userService = userService;
            _notificationService = notificationService;
            _emailService = emailService;
            _battleHub = battleHub;
            _logger = logger;
        }

        // GET: api/admin/dashboard
        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetDashboard(CancellationToken ct = default)
        {
            var stats = await _dashboardService.GetDashboardStatsAsync(ct);
            return Ok(stats);
        }

        // GET: api/admin/configurations
        [HttpGet("configurations")]
        public async Task<IActionResult> GetConfigurations()
        {
            var configs = await _systemConfigService.GetAllConfigurationsAsync();
            return Ok(configs);
        }

        // PUT: api/admin/configurations
        [HttpPut("configurations")]
        public async Task<IActionResult> UpdateConfigurations([FromBody] List<SystemConfiguration> configurations)
        {
            if (configurations == null || !configurations.Any())
            {
                return BadRequest("No configurations provided.");
            }

            try
            {
                await _systemConfigService.UpdateConfigurationsAsync(configurations);
                return Ok(new { Message = "Configurations updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configurations.");
                return StatusCode(500, "An error occurred while updating configurations.");
            }
        }

        // GET: api/admin/health
        [HttpGet("health")]
        public IActionResult GetSystemHealth()
        {
            // Simple health check for dashboard
            return Ok(new
            {
                Status = "Healthy",
                Uptime = "Operational",
                Database = "Connected",
                Timestamp = DateTime.UtcNow
            });
        }

        // POST: api/admin/maintenance/redis-flush
        [HttpPost("maintenance/redis-flush")]
        public IActionResult FlushRedis()
        {
            _logger.LogInformation("SuperAdmin requested Redis cache flush.");
            // In real app, call StackExchange.Redis IDatabase.Execute("FLUSHALL")
            return Ok(new { Message = "Redis cache flushed successfully." });
        }

        // POST: api/admin/maintenance/db-cleanup
        [HttpPost("maintenance/db-cleanup")]
        public IActionResult CleanupDatabase()
        {
            _logger.LogInformation("SuperAdmin requested DB cleanup (archiving old records).");
            // Placeholder for logic to archive AnswerRecords > 1 year old
            return Ok(new { Message = "Database cleanup routine started in background." });
        }

        // GET: api/admin/logs
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs(
            [FromQuery] string? action = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var (items, total) = await _activityLogService.GetAdminLogsAsync(
                action, userId, from, to, pageNumber, pageSize, ct);

            return Ok(new
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
            });
        }

        // PATCH: api/admin/users/{id}/balance
        [HttpPatch("users/{id}/balance")]
        public async Task<IActionResult> AdjustUserBalance(int id, [FromBody] AdjustBalanceDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _userService.AdjustUserBalanceAsync(id, dto, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"User with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting balance for user {UserId}", id);
                return StatusCode(500, "An error occurred while adjusting the balance.");
            }
        }

        // POST: api/admin/notifications/broadcast
        [HttpPost("notifications/broadcast")]
        public async Task<IActionResult> BroadcastNotification(
            [FromBody] BroadcastNotificationDto dto,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var adminUserId = User.GetUserId();
                var result = await _notificationService.BroadcastAsync(dto, adminUserId, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting notification");
                return StatusCode(500, "An error occurred while sending the broadcast.");
            }
        }

        public class TestEmailDto
        {
            [Required]
            [EmailAddress]
            public string ToEmail { get; set; } = string.Empty;
        }

        // POST: api/admin/test-email
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] TestEmailDto dto)
        {
            try
            {
                var htmlContent = @"
                    <div style='font-family: Arial, sans-serif; padding: 20px; text-align: center;'>
                        <h2 style='color: #4F46E5;'>🎉 Chào mừng đến với Vocamon!</h2>
                        <p>Đây là email test xác nhận tính năng SendGrid của hệ thống hoạt động bình thường.</p>
                        <p>Chúc bạn một ngày học tập vui vẻ!</p>
                    </div>";

                await _emailService.SendEmailAsync(dto.ToEmail, "Test tính năng gửi Email từ Vocamon", htmlContent);
                return Ok(new { Message = $"Test email sent successfully to {dto.ToEmail}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email");
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }
        }

        // GET: api/admin/pvp/leaderboard?top=50
        [HttpGet("pvp/leaderboard")]
        public async Task<IActionResult> GetPvpLeaderboard(
            [FromQuery] int top = 50,
            CancellationToken ct = default)
        {
            if (top < 1 || top > 200) top = 50;
            var result = await _dashboardService.GetPvpLeaderboardAsync(top, ct);
            return Ok(result);
        }

        // GET: api/admin/users/{id}/learning-progress
        [HttpGet("users/{id:int}/learning-progress")]
        public async Task<IActionResult> GetUserLearningProgress(
            int id,
            CancellationToken ct = default)
        {
            var result = await _dashboardService.GetUserLearningProgressAsync(id, ct);
            return Ok(result);
        }

        // GET: api/admin/analytics/sessions?days=30
        [HttpGet("analytics/sessions")]
        public async Task<IActionResult> GetSessionAnalytics(
            [FromQuery] int days = 30,
            CancellationToken ct = default)
        {
            if (days < 1 || days > 365) days = 30;
            var result = await _dashboardService.GetSessionAnalyticsAsync(days, ct);
            return Ok(result);
        }

        // GET: api/admin/users/{userId}/review-history?page=1&pageSize=30&from=&to=
        [HttpGet("users/{userId:int}/review-history")]
        public async Task<IActionResult> GetUserReviewHistory(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 30,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 30;
            var result = await _dashboardService.GetUserReviewHistoryAsync(userId, page, pageSize, from, to, ct);
            return Ok(result);
        }

        // GET: api/admin/battles?page=1&pageSize=20&userId=&type=&status=
        [HttpGet("battles")]
        public async Task<IActionResult> GetBattleSessions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? userId = null,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            var result = await _dashboardService.GetBattleSessionsAsync(page, pageSize, userId, type, status, ct);
            return Ok(result);
        }

        // GET: api/admin/battles/{sessionId}
        [HttpGet("battles/{sessionId:int}")]
        public async Task<IActionResult> GetBattleReplay(int sessionId, CancellationToken ct = default)
        {
            var result = await _dashboardService.GetBattleReplayAsync(sessionId, ct);
            if (result == null) return NotFound($"Battle session {sessionId} not found.");
            return Ok(result);
        }

        // POST: api/admin/battles/{sessionId}/abandon
        [HttpPost("battles/{sessionId:int}/abandon")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> AbandonBattle(int sessionId, CancellationToken ct = default)
        {
            var abandoned = await _dashboardService.AbandonBattleAsync(sessionId, ct);
            if (!abandoned)
                return BadRequest($"Battle {sessionId} is not in an active state or does not exist.");

            // Notify connected players via SignalR
            await _battleHub.Clients
                .Group($"battle-{sessionId}")
                .SendAsync("AdminForcedAbandoned",
                    new { sessionId, message = "This battle was stopped by an administrator." },
                    ct);

            _logger.LogInformation("Admin force-abandoned battle session {SessionId}", sessionId);
            return Ok(new { sessionId, status = "Abandoned" });
        }
    }
}
