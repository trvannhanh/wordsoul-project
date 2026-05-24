using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.BackgroundServices
{
    public class LogCleanupBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LogCleanupBackgroundWorker> _logger;

        public LogCleanupBackgroundWorker(
            IServiceScopeFactory scopeFactory, 
            ILogger<LogCleanupBackgroundWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LogCleanupBackgroundWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();

                    // Lấy số ngày lưu log từ cấu hình
                    var retentionConfig = await dbContext.SystemConfigurations
                        .FirstOrDefaultAsync(c => c.Key == "LogRetentionDays", stoppingToken);
                    
                    int retentionDays = 7; // mặc định 7 ngày
                    if (retentionConfig != null && int.TryParse(retentionConfig.Value, out int days))
                    {
                        retentionDays = days;
                    }

                    var thresholdDate = DateTime.UtcNow.AddDays(-retentionDays);

                    var deletedCount = await dbContext.SystemLogs
                        .Where(l => l.Timestamp < thresholdDate)
                        .ExecuteDeleteAsync(stoppingToken);

                    _logger.LogInformation("Deleted {DeletedCount} old system logs (older than {RetentionDays} days).", deletedCount, retentionDays);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up old system logs.");
                }

                // Chạy mỗi 24 giờ
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
