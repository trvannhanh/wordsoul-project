using Microsoft.AspNetCore.SignalR;
using WordSoul.Api.Hubs;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Services
{
    public class SignalRNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationAsync(int userId, object notification)
        {
            try
            {
                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation(
                    "Sent realtime notification to user {UserId}",
                    userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send realtime notification to user {UserId}",
                    userId);

                // Không throw để không ảnh hưởng business logic
                // Có thể log hoặc gửi qua queue để retry
            }
        }
    }
}
