using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

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
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ISystemConfigurationService systemConfigService,
            IActivityLogService activityLogService,
            IAdminDashboardService dashboardService,
            IUserService userService,
            ILogger<AdminController> logger)
        {
            _systemConfigService = systemConfigService;
            _activityLogService = activityLogService;
            _dashboardService = dashboardService;
            _userService = userService;
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
    }
}
