using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSoul.Application.Common
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
