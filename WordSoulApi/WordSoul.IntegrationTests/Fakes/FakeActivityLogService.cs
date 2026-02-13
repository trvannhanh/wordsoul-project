
using WordSoul.Application.DTOs;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeActivityLogService : IActivityLogService
    {
        public Task CreateActivityLogAsync(
            int userId,
            string action,
            string details,
            CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<List<ActivityLogDto>> GetUserActivityLogsAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            return Task.FromResult(new List<ActivityLogDto>());
        }

        public Task<List<ActivityLogDto>> GetAllActivityLogsAsync(
            string? action = null,
            DateTime? fromDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            return Task.FromResult(new List<ActivityLogDto>());
        }
    }
}
