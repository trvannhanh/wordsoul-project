using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakePetBuffService : IPetBuffService
    {
        public Task<PetBuffDto?> GetActivePetBuffAsync(int userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
