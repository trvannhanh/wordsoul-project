using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.BackgroundServices
{
    public class EmailReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailReminderBackgroundService> _logger;

        public EmailReminderBackgroundService(IServiceProvider serviceProvider, ILogger<EmailReminderBackgroundService> logger)
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
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        // Tính thời điểm 3 ngày trước
                        var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

                        var currentHour = DateTime.UtcNow.Hour;

                        // Tìm những user:
                        // - IsActive = true
                        // - LastActiveAt < threeDaysAgo
                        // - Chưa từng được gửi email nhắc nhở (LastReminderEmailSentAt == null) HOẶC lần nhắc cuối cùng cách đây > 7 ngày (tránh spam)
                        // - PreferredStudyHour khớp với giờ hiện tại (hoặc fallback 13 nếu null)
                        var inactiveUsers = await context.Users
                            .Include(u => u.UserOwnedPets)
                            .ThenInclude(up => up.Pet)
                            .Where(u => u.IsActive && u.LastActiveAt < threeDaysAgo && 
                                        (u.LastReminderEmailSentAt == null || u.LastReminderEmailSentAt < DateTime.UtcNow.AddDays(-7)) &&
                                        (u.PreferredStudyHour ?? 13) == currentHour)
                            .ToListAsync(stoppingToken);

                        foreach (var user in inactiveUsers)
                        {
                            var activePetName = user.UserOwnedPets?.FirstOrDefault(p => p.IsActive)?.Pet?.Name ?? "Vocamon";

                            string subject = $"Vocamon {activePetName} đang nhớ bạn!";
                            string htmlContent = $@"
                                <h2>Chào {user.Username ?? "bạn"},</h2>
                                <p>Đã vài ngày rồi bạn chưa vào ứng dụng.</p>
                                <p><strong>{activePetName}</strong> đang rất buồn và đói điểm kinh nghiệm (XP) đấy! Hãy vào học ngay để nhận thưởng và chơi cùng {activePetName} nhé.</p>
                                <br/>
                                <a href='https://vocamon.com' style='background-color:#FFD700;color:#000;padding:10px 20px;text-decoration:none;border-radius:5px;font-weight:bold;'>Vào thăm {activePetName} ngay</a>
                                <br/><br/>
                                <p>Đội ngũ Vocamon</p>";

                            await emailService.SendEmailAsync(user.Email, subject, htmlContent);

                            // Cập nhật lại thời gian đã gửi email
                            user.LastReminderEmailSentAt = DateTime.UtcNow;
                        }

                        if (inactiveUsers.Any())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Sent reminder emails to {inactiveUsers.Count} inactive users.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing EmailReminderBackgroundService");
                }

                // Chạy 1 tiếng 1 lần để check các khung giờ
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
