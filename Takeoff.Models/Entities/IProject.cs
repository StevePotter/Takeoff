
namespace Takeoff.Data
{
    public interface IProject
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
    }
}