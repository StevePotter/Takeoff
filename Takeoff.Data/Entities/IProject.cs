
using System;

namespace Takeoff.Data
{
    public interface IProject : ILatestChangeAware
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
    }

}