using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.Services
{
    public class FcmService : IFcmService
    {
        private readonly ILogger<FcmService> _logger;

        public FcmService(ILogger<FcmService> logger)
        {
            _logger = logger;
        }

        public async Task SendPushNotificationAsync(string fcmToken, string title, string body, string? actionUrl = null)
        {
            try
            {
                if (string.IsNullOrEmpty(fcmToken))
                {
                    _logger.LogWarning("FCM Token is empty. Cannot send push notification.");
                    return;
                }

                var message = new Message()
                {
                    Token = fcmToken,
                    Data = new Dictionary<string, string>()
                    {
                        { "title", title },
                        { "body", body },
                        { "actionUrl", actionUrl ?? "/" }
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"Successfully sent FCM message: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FCM notification");
            }
        }
    }
}
