

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using WordSoul.Application.Services;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Tests.Performance
{
    public class ActivityLogServicePerformanceTests
    {
        private ActivityLogService CreateService(string dbName)
        {
            var options = new DbContextOptionsBuilder<WordSoulDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new WordSoulDbContext(options);
            var uow = new UnitOfWork(context);
            var cache = new MemoryCache(new MemoryCacheOptions());

            return new ActivityLogService(
                uow,
                cache,
                new NullLogger<ActivityLogService>());
        }


        [Fact]
        public async Task CreateActivityLog_SingleWrite_ShouldBeFast()
        {
            var service = CreateService(nameof(CreateActivityLog_SingleWrite_ShouldBeFast));

            var stopwatch = Stopwatch.StartNew();

            await service.TrackUserLoginAsync(1);

            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            Assert.True(elapsedMs < 100,
                $"Single write took too long: {elapsedMs} ms");
        }


        [Fact]
        public async Task CreateActivityLog_100SequentialWrites_ShouldBeAcceptable()
        {
            var service = CreateService(nameof(CreateActivityLog_100SequentialWrites_ShouldBeAcceptable));

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                await service.TrackUserLoginAsync(1);
            }

            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            Assert.True(elapsedMs < 2000,
                $"100 sequential writes too slow: {elapsedMs} ms");
        }


        [Fact]
        public async Task CreateActivityLog_100ConcurrentWrites_ShouldNotCrash()
        {
            var stopwatch = Stopwatch.StartNew();

            var tasks = Enumerable.Range(0, 100)
                .Select(i =>
                {
                    var service = CreateService(
                        nameof(CreateActivityLog_100ConcurrentWrites_ShouldNotCrash) + "_" + i);

                    return service.TrackUserLoginAsync(1);
                });

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            Assert.True(elapsedMs < 3000,
                $"100 concurrent writes too slow: {elapsedMs} ms");
        }
    }
}

