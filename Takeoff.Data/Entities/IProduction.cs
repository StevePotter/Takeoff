
using System;

namespace Takeoff.Data
{
    public interface IProduction : ILatestChangeAware
    {
        string Title { get; set; }

        /// <summary>
        /// When set, indicates the ID of the direct child ImageThing that represents a logo shown next to the title.
        /// </summary>
        int? LogoImageId { get; set; }

        /// <summary>
        /// Indicates whether this is a sample production created when the user signed up.
        /// </summary>
        bool IsSample { get; set; }

        string FilesLocation { get; set; }
        
        IChange[] Activity { get; }

        int Id { get; }

        /// <summary>
        /// The ID in ChangeDetails that is the latest change to happen to this thing or its descendants.
        /// </summary>
        int? LastChangeId { get; }

        /// <summary>
        /// The date of the latest change that occured for this thing.
        /// </summary>
        DateTime? LastChangeDate { get; }

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