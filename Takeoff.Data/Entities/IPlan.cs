using System;
using System.IO;
using Takeoff.Models;

namespace Takeoff.Data
{
    public interface IPlan
    {
        string Id { get; set; }

        DateTime? CreatedOn { get; set; }

        string Title { get; set; }

        string Notes { get; set; }

        /// <summary>
        /// The total number of videos allowed in the system at a given time.
        /// </summary>
        int? VideosTotalMaxCount { get; set; }

        /// <summary>
        /// The maximum number of videos that can be uploaded in a given billing cycle.  This prevents lots of churn, which results in high costs.
        /// </summary>
        int? VideosPerBillingCycleMax { get; set; }

        /// <summary>
        /// The total number of files allowed in the system at a given time.
        /// </summary>
        int? AssetsTotalMaxCount { get; set; }

        /// <summary>
        /// The total number of files, including past deleted files, allowed at a given time.  This is useful for demos and whatnot.
        /// </summary>
        int? AssetsAllTimeMaxCount { get; set; }

        /// <summary>
        /// The total number of productions allowed at a given time.
        /// </summary>
        int? ProductionLimit { get; set; }

        /// <summary>
        /// Number of maximum seconds allowed for a particular video.  If this is null, a global value of 1hr will be used.  For an infinite plan, just use a gigantic number.
        /// </summary>
        int? VideoDurationLimit { get; set; }

        bool AllowSignups { get; set; }

        int PriceInCents { get; set; }

        bool IsFree { get; set; }

        /// <summary>
        /// The maximum allowed size for a video upload.
        /// </summary>
        FileSize? VideoFileMaxSize { get; set; }

        /// <summary>
        /// The maximum allowed size for an asset upload.
        /// </summary>
        FileSize? AssetFileMaxSize { get; set; }

        /// <summary>
        /// The maximum allowed size for all asset files.
        /// </summary>
        FileSize? AssetsTotalMaxSize { get; set; }

        PlanInterval TrialIntervalType { get; set; }

        int TrialIntervalLength { get; set; }

        PlanInterval BillingIntervalType { get; set; }

        int BillingIntervalLength { get; set; }
                
    }

}