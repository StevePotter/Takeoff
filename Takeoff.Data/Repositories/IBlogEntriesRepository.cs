using System;
using System.Collections.Generic;

namespace Takeoff.Data
{
    public interface IBlogEntriesRepository
    {
        IEnumerable<IBlogEntry> Get();
    }
}
