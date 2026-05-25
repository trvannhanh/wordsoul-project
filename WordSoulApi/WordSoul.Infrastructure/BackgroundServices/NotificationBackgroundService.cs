using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Enums;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.BackgroundServices
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;

        private readonly string[] _reviewMessages = new[]
        {
            "Cấp báo! Bộ não sắp quên {0} từ vựng này rồi!",
            "Vocamon của bạn đang đói, hãy ôn {0} từ vựng để cho chúng ăn nào!",
            "Bạn có {0} từ cần ôn tập. Đừng để công sức học đổ sông đổ biển nhé!",
            "Chỉ mất vài phút để ôn lại {0} từ. Vào học ngay để duy trì phong độ!"
        };
        private readonly Random _random = new();

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

                        var currentHour = DateTime.UtcNow.Hour;

                        var usersWithDueVocab = await context.UserVocabularyProgresses
                            .Include(p => p.User)
                            .Where(p => p.NextReviewTime <= DateTime.UtcNow && 
                                        p.User != null && p.User.IsActive && 
                                        (p.User.PreferredStudyHour ?? 13) == currentHour)
                            .GroupBy(p => p.UserId)
                            .Select(g => new { UserId = g.Key, Count = g.Count() })
                            .ToListAsync(stoppingToken);

                        foreach (var item in usersWithDueVocab)
                        {
                            if (item.Count > 0)
                            {
                                // Check nếu đã có thông báo chưa đọc tương tự trong 1 giờ qua để tránh duplicate
                                var existing = await context.Notifications
                                    .AnyAsync(n => n.UserId == item.UserId && n.Type == NotificationType.Review && !n.IsRead && n.CreatedAt > DateTime.UtcNow.AddHours(-1));
                                if (!existing)
                                {
                                    string template = _reviewMessages[_random.Next(_reviewMessages.Length)];
                                    string message = string.Format(template, item.Count);
                                    string actionUrl = "/home"; // Dẫn về màn hình Dashboard để học
                                    await notificationService.CreateNotificationAsync(item.UserId, "Ôn tập từ vựng", message, NotificationType.Review, actionUrl);
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
