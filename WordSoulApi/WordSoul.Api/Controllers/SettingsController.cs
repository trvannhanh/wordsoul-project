using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Application.DTOs.SystemConfiguration;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Api.Extensions;
using WordSoul.Domain.Entities;

namespace WordSoul.Api.Controllers
{
    [Route("api/settings")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    public class SettingsController : ControllerBase
    {
        private readonly ISystemConfigurationService _systemConfigService;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            ISystemConfigurationService systemConfigService,
            ILogger<SettingsController> logger)
        {
            _systemConfigService = systemConfigService;
            _logger = logger;
        }

        // GET: api/settings/public (Anonymous access for web app branding & configs)
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicSettings()
        {
            try
            {
                var configs = await _systemConfigService.GetAllConfigurationsAsync();
                // Only return general configurations to the public client to prevent leaking sensitive system values
                var publicConfigs = configs
                    .Where(c => string.Equals(c.Category, "GENERAL", StringComparison.OrdinalIgnoreCase))
                    .Select(c => new { c.Key, c.Value })
                    .ToList();

                return Ok(publicConfigs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching public settings.");
                return StatusCode(500, "An error occurred while fetching public settings.");
            }
        }

        // GET: api/settings (Admin/SuperAdmin only)
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllSettings()
        {
            var configs = await _systemConfigService.GetAllConfigurationsAsync();
            return Ok(configs);
        }

        // GET: api/settings/{key} (Admin/SuperAdmin only)
        [HttpGet("{key}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetSettingByKey(string key)
        {
            var config = await _systemConfigService.GetConfigurationByKeyAsync(key);
            if (config == null)
            {
                return NotFound($"Configuration with key '{key}' not found.");
            }
            return Ok(config);
        }

        // POST: api/settings (SuperAdmin only)
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateSetting([FromBody] CreateSystemConfigurationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var username = User.GetUsername();
                var config = new SystemConfiguration
                {
                    Key = dto.Key,
                    Value = dto.Value,
                    DataType = dto.DataType,
                    Description = dto.Description,
                    Category = dto.Category ?? "GENERAL",
                    LastUpdatedBy = string.IsNullOrEmpty(username) ? "SuperAdmin" : username,
                    LastUpdatedAt = DateTime.UtcNow
                };

                var created = await _systemConfigService.CreateConfigurationAsync(config);
                return CreatedAtAction(nameof(GetSettingByKey), new { key = created.Key }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating setting '{Key}'", dto.Key);
                return StatusCode(500, "An error occurred while creating the setting.");
            }
        }

        // PUT: api/settings/{key} (SuperAdmin only)
        [HttpPut("{key}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSystemConfigurationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var username = User.GetUsername();
                var config = new SystemConfiguration
                {
                    Key = key,
                    Value = dto.Value,
                    DataType = "", // Will use existing DataType for validation inside service
                    Description = dto.Description,
                    Category = dto.Category,
                    LastUpdatedBy = string.IsNullOrEmpty(username) ? "SuperAdmin" : username,
                    LastUpdatedAt = DateTime.UtcNow
                };

                var updated = await _systemConfigService.UpdateConfigurationAsync(key, config);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setting '{Key}'", key);
                return StatusCode(500, "An error occurred while updating the setting.");
            }
        }

        // DELETE: api/settings/{key} (SuperAdmin only)
        [HttpDelete("{key}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteSetting(string key)
        {
            try
            {
                var deleted = await _systemConfigService.DeleteConfigurationAsync(key);
                if (!deleted)
                {
                    return NotFound($"Configuration with key '{key}' not found.");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting setting '{Key}'", key);
                return StatusCode(500, "An error occurred while deleting the setting.");
            }
        }
    }
}
