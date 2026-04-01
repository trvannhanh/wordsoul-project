using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WordSoul.Application.DTOs.Battle;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Hubs
{
    /// <summary>
    /// SignalR Hub cho Real-time Arena Battle (PvE Gym + PvP).
    /// Client kết nối sau khi có battleSessionId từ REST API.
    /// </summary>
    [Authorize]
    public class BattleHub : Hub
    {
        private readonly IArenaBattleService _arena;
        private readonly ILogger<BattleHub> _logger;

        public BattleHub(IArenaBattleService arena, ILogger<BattleHub> logger)
        {
            _arena = arena;
            _logger = logger;
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private int GetUserId() =>
            int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static string RoomGroup(int sessionId) => $"battle-{sessionId}";

        // ═══════════════════════════════════════════════════════════
        // PvE – Gym Battle
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Player vào phòng PvE battle. Bot sẵn sàng ngay → battle bắt đầu ngay.
        /// </summary>
        public async Task PlayerReady(int battleSessionId)
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(battleSessionId));
            _logger.LogInformation("User {U} joined BattleHub room {S} (PvE)", userId, battleSessionId);

            var result = await _arena.StartBattleAsync(battleSessionId, userId);
            if (result != null)
            {
                await Clients.Caller.SendAsync("BattleStarted", result);
            }
        }

        /// <summary>
        /// Player gửi câu trả lời cho một round (PvE).
        /// </summary>
        public async Task SubmitAnswer(SubmitRoundAnswerDto dto)
        {
            var userId = GetUserId();
            var roundResult = await _arena.SubmitAnswerAsync(dto, userId);

            if (roundResult != null)
            {
                await Clients.Group(RoomGroup(dto.BattleSessionId))
                    .SendAsync("RoundResult", roundResult.RoundResult);

                if (roundResult.BattleEnded != null)
                {
                    await Clients.Group(RoomGroup(dto.BattleSessionId))
                        .SendAsync("BattleEnded", roundResult.BattleEnded);
                    return;
                }

                await Task.Delay(2500);
                if (roundResult.NextQuestion != null)
                {
                    await Clients.Group(RoomGroup(dto.BattleSessionId))
                        .SendAsync("NextQuestion", roundResult.NextQuestion);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PvP – Player vs Player
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Player vào phòng PvP. Khi cả 2 ready → BattleStarted broadcast cho cả 2.
        /// </summary>
        public async Task PlayerReadyPvP(int battleSessionId)
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(battleSessionId));
            _logger.LogInformation("User {U} joined BattleHub room {S} (PvP)", userId, battleSessionId);

            var result = await _arena.StartPvpBattleAsync(battleSessionId, userId, Context.ConnectionId);

            if (result == null)
            {
                // Chưa đủ 2 người – thông báo cho người vừa vào biết đang chờ
                await Clients.Caller.SendAsync("WaitingOpponent", new { message = "Đang chờ đối thủ vào phòng..." });
            }
            else
            {
                // Cả 2 đã sẵn sàng – broadcast BattleStarted cho toàn phòng
                await Clients.Group(RoomGroup(battleSessionId)).SendAsync("BattleStarted", result);
            }
        }

        /// <summary>
        /// Player gửi câu trả lời PvP. Server buffer, khi đủ 2 bên mới resolve round.
        /// </summary>
        public async Task SubmitPvpAnswer(SubmitRoundAnswerDto dto)
        {
            var userId = GetUserId();
            var result = await _arena.SubmitPvpAnswerAsync(dto, userId);

            if (result == null)
            {
                // Chưa đủ 2 bên – báo waiting cho người submit trước
                await Clients.Caller.SendAsync("WaitingOpponent", new { message = "Đang chờ đối thủ trả lời..." });
                return;
            }

            // Cả 2 đã submit → broadcast kết quả round
            await Clients.Group(RoomGroup(dto.BattleSessionId))
                .SendAsync("RoundResult", result.RoundResult);

            if (result.BattleEnded != null)
            {
                await Clients.Group(RoomGroup(dto.BattleSessionId))
                    .SendAsync("BattleEnded", result.BattleEnded);
                return;
            }

            // Gửi câu hỏi tiếp theo sau 2.5 giây
            await Task.Delay(2500);
            if (result.NextQuestion != null)
            {
                await Clients.Group(RoomGroup(dto.BattleSessionId))
                    .SendAsync("NextQuestion", result.NextQuestion);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Disconnect Handling
        // ═══════════════════════════════════════════════════════════

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogWarning("BattleHub client {C} disconnected: {E}",
                Context.ConnectionId, exception?.Message);

            // Xử lý PvP forfeit khi disconnect
            var battleEnded = await _arena.ForfeitPvpBattleAsync(Context.ConnectionId);
            if (battleEnded != null)
            {
                await Clients.Group($"battle-{battleEnded.BattleSessionId}")
                    .SendAsync("OpponentForfeited", battleEnded);

                _logger.LogInformation(
                    "PvP session {S} ended by forfeit (disconnect). P1Won={W}",
                    battleEnded.BattleSessionId, battleEnded.P1Won);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
