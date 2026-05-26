using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WordSoul.Application.DTOs.Battle;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Enums;

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

        /// <summary>
        /// Lấy lịch sử thi đấu của người dùng hiện tại (bao gồm Gym Battles và PvP Battles).
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] BattleType? type, [FromQuery] int? gymLeaderId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            var history = await _arena.GetBattleHistoryAsync(userId, type, gymLeaderId, page, pageSize);
            return Ok(history);
        }

        /// <summary>
        /// Lấy chi tiết lịch sử một trận đấu.
        /// </summary>
        [HttpGet("history/{sessionId:int}")]
        public async Task<IActionResult> GetHistoryDetail(int sessionId)
        {
            var userId = GetUserId();
            var detail = await _arena.GetBattleHistoryDetailAsync(sessionId, userId);
            if (detail == null)
                return NotFound(new { error = "Không tìm thấy chi tiết trận đấu hoặc bạn không có quyền xem." });

            return Ok(detail);
        }
    }
}
