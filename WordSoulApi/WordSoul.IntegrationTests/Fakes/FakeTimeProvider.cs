using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSoul.Application.Common;

namespace WordSoul.IntegrationTests.Fakes
{
    public class FakeTimeProvider : ITimeProvider
    {
        public DateTime UtcNow { get; set; }
    }
}
