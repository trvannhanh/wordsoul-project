using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordSoul.Domain.Entities;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.BackgroundServices
{
    public class SystemLogBackgroundWorker : BackgroundService
    {
        private readonly SystemLogQueue _logQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SystemLogBackgroundWorker> _logger;

        public SystemLogBackgroundWorker(
            SystemLogQueue logQueue, 
            IServiceScopeFactory scopeFactory, 
            ILogger<SystemLogBackgroundWorker> logger)
        {
            _logQueue = logQueue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SystemLogBackgroundWorker is running.");

            await foreach (var log in _logQueue.DequeueAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();
                    
                    dbContext.SystemLogs.Add(log);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save SystemLog to database.");
                }
            }
        }
    }
}
