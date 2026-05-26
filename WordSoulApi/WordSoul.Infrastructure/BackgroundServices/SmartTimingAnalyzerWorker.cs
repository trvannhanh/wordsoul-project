using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.BackgroundServices
{
    public class SmartTimingAnalyzerWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SmartTimingAnalyzerWorker> _logger;

        public SmartTimingAnalyzerWorker(IServiceProvider serviceProvider, ILogger<SmartTimingAnalyzerWorker> logger)
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
                    _logger.LogInformation("SmartTimingAnalyzerWorker is starting analysis...");
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();
                        
                        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                        
                        // Lấy tất cả user
                        var users = await context.Users.Where(u => u.IsActive).ToListAsync(stoppingToken);
                        
                        int updateCount = 0;
                        foreach(var user in users)
                        {
                            // Lấy tất cả các session của user trong 30 ngày qua
                            var sessions = await context.LearningSessions
                                .Where(s => s.UserId == user.Id && s.StartTime >= thirtyDaysAgo)
                                .Select(s => s.StartTime.Hour)
                                .ToListAsync(stoppingToken);
                                
                            if (sessions.Any())
                            {
                                // Tìm giờ xuất hiện nhiều nhất
                                var mostFrequentHour = sessions
                                    .GroupBy(h => h)
                                    .OrderByDescending(g => g.Count())
                                    .Select(g => g.Key)
                                    .FirstOrDefault();
                                    
                                if (user.PreferredStudyHour != mostFrequentHour)
                                {
                                    user.PreferredStudyHour = mostFrequentHour;
                                    updateCount++;
                                }
                            }
                        }
                        
                        if (updateCount > 0)
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"SmartTimingAnalyzerWorker updated PreferredStudyHour for {updateCount} users.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in SmartTimingAnalyzerWorker");
                }

                // Chạy mỗi ngày 1 lần. Delay 24 tiếng.
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
