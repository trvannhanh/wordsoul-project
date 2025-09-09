using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(IServiceProvider serviceProvider, ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var usersWithDueVocab = await context.UserVocabularyProgresses
                        .Where(p => p.NextReviewTime <= DateTime.UtcNow)
                        .GroupBy(p => p.UserId)
                        .Select(g => new { UserId = g.Key, Count = g.Count() })
                        .ToListAsync();

                        foreach (var item in usersWithDueVocab)
                        {
                            if (item.Count > 0)
                            {
                                // Check nếu đã có thông báo chưa đọc tương tự trong 1 giờ qua để tránh duplicate
                                var existing = await context.Notifications
                                    .AnyAsync(n => n.UserId == item.UserId && n.Type == NotificationType.Review && !n.IsRead && n.CreatedAt > DateTime.UtcNow.AddHours(-1));
                                if (!existing)
                                {
                                    string message = $"Bạn có {item.Count} từ cần ôn tập.";
                                    await notificationService.CreateNotificationAsync(item.UserId, "Ôn tập từ vựng", message, NotificationType.Review);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while generating notifications");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

            }
        }
    }
}
