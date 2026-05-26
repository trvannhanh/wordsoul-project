using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.Services
{
    public class FcmService : IFcmService
    {
        private readonly ILogger<FcmService> _logger;
        private readonly IConfiguration _configuration;

        public FcmService(ILogger<FcmService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

                var resolvedActionUrl = actionUrl ?? "/";
                if (resolvedActionUrl.StartsWith("/"))
                {
                    var webAppUrl = _configuration["AppSettings:WebAppUrl"]?.TrimEnd('/')
                                   ?? _configuration["AllowedOrigins"]?.Split(',')[0]?.Trim()?.TrimEnd('/')
                                   ?? "http://localhost:5173";
                    resolvedActionUrl = webAppUrl + resolvedActionUrl;
                }

                var message = new Message()
                {
                    Token = fcmToken,
                    Data = new Dictionary<string, string>()
                    {
                        { "title", title },
                        { "body", body },
                        { "actionUrl", resolvedActionUrl }
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
