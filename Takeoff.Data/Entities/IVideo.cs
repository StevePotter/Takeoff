using System;

namespace Takeoff.Data
{
    public interface IVideo : ITypicalEntity
    {
        string Title { get; set; }

        /// <summary>
        /// Indicates whether this is a sample file created when the user signed up or whatever.  This doesn't count toward plan usage.
        /// </summary>
        bool IsSample { get; set; }

        bool IsSourceDownloadable { get; set; }

        /// <summary>
        /// The duration of the video.
        /// </summary>
        TimeSpan? Duration { get; set; }

        bool HasVideo { get; }

        /// <summary>
        /// The encrypted guest password.  If not set, guest access is not permitted.
        /// </summary>
        string GuestPassword { get; set; }

        /// <summary>
        /// Whether guests can write comments for the video.
        /// </summary>
        bool GuestsCanComment { get; set; }

    }
}