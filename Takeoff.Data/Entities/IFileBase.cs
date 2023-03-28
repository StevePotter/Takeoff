namespace Takeoff.Data
{
    public interface IFileBase : ITypicalEntity
    {
        /// <summary>
        /// The named of the file in Takeoff's S3 file system.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// The name of the file when it was uploaded.
        /// </summary>
        string OriginalFileName { get; set; }

        /// <summary>
        /// The s3 bucket and folder of this file.  Example: to-d-projects/11195
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// The file size, in bytes.
        /// </summary>
        int? Bytes { get; set; }

        /// <summary>
        /// Indicates whether this is a sample file created when the user signed up or whatever.  This doesn't count toward plan usage.
        /// </summary>
        bool IsSample { get; set; }

        /// <summary>
        /// When true, the file will be deleted when this thing is deleted.  It is set to false for shared files like for sample projects.  This is not really necessary since IsSample arrived.  todo: remove this
        /// </summary>
        bool DeletePhysicalFile { get; set; }
    }
}