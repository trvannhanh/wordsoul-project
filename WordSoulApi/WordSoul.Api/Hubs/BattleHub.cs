using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WordSoul.Application.DTOs.Battle;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Hubs
{
    /// <summary>
    /// SignalR Hub cho Real-time Arena Battle (PvE Gym + tương lai PvP).
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

        // ── Helpers ───────────────────────────────────────────────────────────

        private int GetUserId() =>
            int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static string RoomGroup(int sessionId) => $"battle-{sessionId}";

        // ── Client → Server ───────────────────────────────────────────────────

        /// <summary>
        /// Player vào phòng battle. Nếu là PvE thì battle tự bắt đầu luôn sau khi player ready.
        /// </summary>
        public async Task PlayerReady(int battleSessionId)
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(battleSessionId));
            _logger.LogInformation("User {U} joined BattleHub room {S}", userId, battleSessionId);

            // Bắt đầu battle (Bot PvE sẵn sàng ngay)
            var result = await _arena.StartBattleAsync(battleSessionId, userId);
            if (result != null)
            {
                await Clients.Caller.SendAsync("BattleStarted", result);
            }
        }

        /// <summary>
        /// Player gửi câu trả lời cho một round.
        /// </summary>
        public async Task SubmitAnswer(SubmitRoundAnswerDto dto)
        {
            var userId = GetUserId();
            var roundResult = await _arena.SubmitAnswerAsync(dto, userId);

            if (roundResult != null)
            {
                // Broadcast kết quả round cho cả 2 bên (hoặc chỉ caller nếu PvE)
                await Clients.Group(RoomGroup(dto.BattleSessionId))
                    .SendAsync("RoundResult", roundResult.RoundResult);

                // Nếu battle đã kết thúc
                if (roundResult.BattleEnded != null)
                {
                    await Clients.Group(RoomGroup(dto.BattleSessionId))
                        .SendAsync("BattleEnded", roundResult.BattleEnded);
                    return;
                }

                // Gửi câu hỏi tiếp theo sau 2.5 giây
                await Task.Delay(2500);
                var nextQ = roundResult.NextQuestion;
                if (nextQ != null)
                {
                    await Clients.Group(RoomGroup(dto.BattleSessionId))
                        .SendAsync("NextQuestion", nextQ);
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogWarning("BattleHub client {C} disconnected: {E}", Context.ConnectionId, exception?.Message);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
