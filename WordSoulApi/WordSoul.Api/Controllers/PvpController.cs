using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WordSoul.Application.DTOs.Battle;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Api.Hubs;

namespace WordSoul.Api.Controllers
{
    /// <summary>
    /// REST API cho PvP Battle.
    /// Flow: POST /create → chia sẻ roomCode → Opponent POST /join → cả 2 kết nối /battleHub → PlayerReadyPvP.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/pvp")]
    public class PvpController : ControllerBase
    {
        private readonly IArenaBattleService _arena;
        private readonly IHubContext<BattleHub> _hubContext;
        private readonly ILogger<PvpController> _logger;

        public PvpController(
            IArenaBattleService arena,
            IHubContext<BattleHub> hubContext,
            ILogger<PvpController> logger)
        {
            _arena = arena;
            _hubContext = hubContext;
            _logger = logger;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Tạo phòng PvP. Trả về { sessionId, roomCode }.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePvpSessionDto dto)
        {
            try
            {
                var result = await _arena.CreatePvpSessionAsync(dto, GetUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Người chơi B vào phòng bằng roomCode + chọn 3 pet.
        /// Sau khi join thành công, server notify P1 qua SignalR "OpponentJoined".
        /// Trả về { sessionId }.
        /// </summary>
        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinPvpSessionDto dto)
        {
            try
            {
                var (sessionId, p1UserId) = await _arena.JoinPvpSessionAsync(dto, GetUserId());

                // Notify P1 qua SignalR group rằng opponent đã vào
                var joiner = HttpContext.User;
                var opponentName = joiner.FindFirstValue(ClaimTypes.Name)
                    ?? joiner.FindFirstValue(ClaimTypes.Email)
                    ?? "Opponent";

                var notifyDto = new OpponentJoinedDto
                {
                    OpponentName = opponentName,
                    AvatarUrl = null,
                    OpponentRating = 1000 // sẽ load từ DB nếu cần
                };

                await _hubContext.Clients
                    .Group($"battle-{sessionId}")
                    .SendAsync("OpponentJoined", notifyDto);

                return Ok(new { sessionId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin ELO/Rating của user hiện tại.
        /// </summary>
        [HttpGet("rating")]
        public async Task<IActionResult> GetRating()
        {
            var rating = await _arena.GetPvpRatingAsync(GetUserId());
            if (rating == null) return NotFound();
            return Ok(rating);
        }

        /// <summary>
        /// Lấy thông tin ELO/Rating của user theo ID.
        /// </summary>
        [HttpGet("rating/{userId:int}")]
        public async Task<IActionResult> GetRatingById(int userId)
        {
            var rating = await _arena.GetPvpRatingAsync(userId);
            if (rating == null) return NotFound();
            return Ok(rating);
        }
    }
}
