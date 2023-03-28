using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takeoff.Data
{
    /// <summary>
    /// Used basically to abstract httpcontext.current.items so things can be done outside of web context easily.
    /// </summary>
    public interface IThreadCacheService
    {
        IDictionary Items { get; } 
    }
}
