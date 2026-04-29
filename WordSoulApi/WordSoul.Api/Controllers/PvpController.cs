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
    /// Flow Room Code: POST /create → chia sẻ roomCode → Opponent POST /join → cả 2 kết nối /battleHub → PlayerReadyPvP.
    /// Flow Matchmaking: POST /queue/join → server ghép cặp → MatchFound (SignalR) → PlayerReadyPvP.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/pvp")]
    public class PvpController : ControllerBase
    {
        private readonly IArenaBattleService _arena;
        private readonly IMatchmakingQueueService _queue;
        private readonly IHubContext<BattleHub> _hubContext;
        private readonly ILogger<PvpController> _logger;

        public PvpController(
            IArenaBattleService arena,
            IMatchmakingQueueService queue,
            IHubContext<BattleHub> hubContext,
            ILogger<PvpController> logger)
        {
            _arena = arena;
            _queue = queue;
            _hubContext = hubContext;
            _logger = logger;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ═══════════════════════════════════════════════════
        // ROOM CODE FLOW
        // ═══════════════════════════════════════════════════

        /// <summary>Tạo phòng PvP. Trả về { sessionId, roomCode }.</summary>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePvpSessionDto dto)
        {
            try
            {
                var result = await _arena.CreatePvpSessionAsync(dto, GetUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        /// <summary>
        /// Người chơi B vào phòng bằng roomCode + chọn 3 pet.
        /// Sau khi join thành công, server notify P1 qua SignalR "OpponentJoined".
        /// </summary>
        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinPvpSessionDto dto)
        {
            try
            {
                // Người chơi B vào phòng bằng roomCode + chọn 3 pet.
                var (sessionId, p1UserId) = await _arena.JoinPvpSessionAsync(dto, GetUserId());

                // Lấy thông tin người chơi B
                var opponentName = User.FindFirstValue(ClaimTypes.Name)
                    ?? User.FindFirstValue(ClaimTypes.Email)
                    ?? "Opponent";

                // Lấy thông tin rating của người chơi B
                var rating = await _arena.GetPvpRatingAsync(GetUserId());

                // Gửi thông tin người chơi B cho người chơi A
                await _hubContext.Clients
                    .Group($"battle-{sessionId}")
                    .SendAsync("OpponentJoined", new OpponentJoinedDto
                    {
                        OpponentName = opponentName,
                        AvatarUrl = null,
                        OpponentRating = rating?.PvpRating ?? 1000
                    });

                return Ok(new { sessionId });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        // ═══════════════════════════════════════════════════
        // MATCHMAKING QUEUE FLOW
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Vào hàng chờ matchmaking ELO-based.
        /// Client cần kết nối BattleHub trước rồi gửi ConnectionId trong body.
        /// Server sẽ push "MatchFound" qua SignalR khi ghép được cặp.
        /// </summary>
        [HttpPost("queue/join")]
        public async Task<IActionResult> JoinQueue([FromBody] JoinQueueDto dto)
        {
            var userId = GetUserId();

            if (dto.SelectedPetIds.Count != 3)
                return BadRequest(new { error = "Bạn phải chọn đúng 3 Pokémon." });

            // Kiểm tra ConnectionId
            if (string.IsNullOrWhiteSpace(dto.ConnectionId))
                return BadRequest(new { error = "ConnectionId không hợp lệ. Hãy kết nối BattleHub trước." });

            // Lấy thông tin rating của người chơi
            var rating = await _arena.GetPvpRatingAsync(userId);
            var pvpRating = rating?.PvpRating ?? 1000;

            // Thêm người chơi vào hàng chờ
            var queueId = _queue.Enqueue(userId, pvpRating, dto.ConnectionId, dto.SelectedPetIds);

            _logger.LogInformation("User {U} (ELO {R}) joined matchmaking queue. QueueId={Q}", userId, pvpRating, queueId);

            return Ok(new QueueJoinedDto { QueueId = queueId, Status = "queued" });
        }

        /// <summary>Rút khỏi hàng chờ matchmaking.</summary>
        [HttpDelete("queue/leave")]
        public IActionResult LeaveQueue([FromQuery] string queueId)
        {
            var removed = _queue.Dequeue(queueId);
            if (!removed)
                return NotFound(new { error = "Không tìm thấy trong hàng chờ." });

            _logger.LogInformation("User {U} left matchmaking queue. QueueId={Q}", GetUserId(), queueId);
            return Ok(new { status = "left" });
        }

        // ═══════════════════════════════════════════════════
        // RATING
        // ═══════════════════════════════════════════════════

        /// <summary>Lấy thông tin ELO/Rating của user hiện tại.</summary>
        [HttpGet("rating")]
        public async Task<IActionResult> GetRating()
        {
            var rating = await _arena.GetPvpRatingAsync(GetUserId());
            if (rating == null) return NotFound();
            return Ok(rating);
        }

        /// <summary>Lấy thông tin ELO/Rating của user theo ID.</summary>
        [HttpGet("rating/{userId:int}")]
        public async Task<IActionResult> GetRatingById(int userId)
        {
            var rating = await _arena.GetPvpRatingAsync(userId);
            if (rating == null) return NotFound();
            return Ok(rating);
        }
    }
}
