using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace WordSoul.Application.Services
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SystemConfigurationService> _logger;

        public SystemConfigurationService(IUnitOfWork unitOfWork, ILogger<SystemConfigurationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<SystemConfiguration>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.SystemConfiguration.GetAllAsync(cancellationToken);
        }

        public async Task<SystemConfiguration?> GetConfigurationByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.SystemConfiguration.GetByKeyAsync(key, cancellationToken);
        }

        public async Task UpdateConfigurationsAsync(IEnumerable<SystemConfiguration> configurations, CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.SystemConfiguration.UpdateBulkAsync(configurations, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully updated system configurations in bulk.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system configurations.");
                throw;
            }
        }
    }
}
