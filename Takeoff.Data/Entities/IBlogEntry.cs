using System;
using System.IO;

namespace Takeoff.Data
{
    /// <summary>
    /// Used on home page to show latest blog entries.
    /// </summary>
    public interface IBlogEntry
    {
        string Link { get; set; }
        string Title { get; set; }
        DateTime DateWritten { get; set; }
        string Description { get; set; }

    }
}