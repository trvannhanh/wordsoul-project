using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WordSoul.Application.DTOs.Battle;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Controllers
{
    /// <summary>
    /// REST API cho Real-time Arena Battle.
    /// Client flow: POST /setup → nhận sessionId → kết nối /battleHub → gọi PlayerReady.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/arena")]
    public class ArenaBattleController : ControllerBase
    {
        private readonly IArenaBattleService _arena;
        private readonly ILogger<ArenaBattleController> _logger;

        public ArenaBattleController(IArenaBattleService arena, ILogger<ArenaBattleController> logger)
        {
            _arena = arena;
            _logger = logger;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Tạo một BattleSession mới cho Gym Battle Arena.
        /// Body: { gymLeaderId, selectedPetIds: [id1, id2, id3] }
        /// Response: { sessionId }
        /// </summary>
        [HttpPost("setup")]
        public async Task<IActionResult> Setup([FromBody] StartArenaBattleRequestDto dto)
        {
            try
            {
                var sessionId = await _arena.CreateSessionAsync(dto, GetUserId());
                return Ok(new { sessionId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
