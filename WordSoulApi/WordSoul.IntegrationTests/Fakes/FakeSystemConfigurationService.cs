using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Admin;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeSystemConfigurationService : ISystemConfigurationService
    {
        public Task<List<SystemConfiguration>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<SystemConfiguration?> GetConfigurationByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateConfigurationsAsync(
            IEnumerable<UpdateSystemConfigurationDto> configurations,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<SystemConfiguration> CreateConfigurationAsync(SystemConfiguration config, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<SystemConfiguration> UpdateConfigurationAsync(string key, SystemConfiguration config, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteConfigurationAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetValueAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default)
            where T : IParsable<T>
        {
            return Task.FromResult(defaultValue);
        }
    }
}
