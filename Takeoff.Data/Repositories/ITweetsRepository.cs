using System;
using System.Collections.Generic;

namespace Takeoff.Data
{
    public interface ITweetsRepository
    {
        IEnumerable<ITweet> Get();
    }
}
