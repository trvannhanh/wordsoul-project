using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class SystemConfigurationRepository : ISystemConfigurationRepository
    {
        private readonly WordSoulDbContext _context;

        public SystemConfigurationRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task<List<SystemConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SystemConfigurations.ToListAsync(cancellationToken);
        }

        public async Task<SystemConfiguration?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _context.SystemConfigurations.FindAsync(new object[] { key }, cancellationToken);
        }

        public Task UpdateAsync(SystemConfiguration config, CancellationToken cancellationToken = default)
        {
            config.LastUpdatedAt = DateTime.UtcNow;
            _context.SystemConfigurations.Update(config);
            return Task.CompletedTask;
        }

        public Task UpdateBulkAsync(IEnumerable<SystemConfiguration> configs, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            foreach (var config in configs)
            {
                config.LastUpdatedAt = now;
            }
            _context.SystemConfigurations.UpdateRange(configs);
            return Task.CompletedTask;
        }

        public async Task AddAsync(SystemConfiguration config, CancellationToken cancellationToken = default)
        {
            await _context.SystemConfigurations.AddAsync(config, cancellationToken);
        }

        public Task DeleteAsync(SystemConfiguration config, CancellationToken cancellationToken = default)
        {
            _context.SystemConfigurations.Remove(config);
            return Task.CompletedTask;
        }
    }
}
