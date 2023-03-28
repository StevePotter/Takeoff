using System;
using System.IO;

namespace Takeoff.Data
{
    public interface IAccount: ITypicalEntity
    {
        /// <summary>
        /// When set, indicates the ID of the direct child ImageThing that represents a logo shown next to the title.
        /// </summary>
        int? LogoImageId { get; set; }

        /// <summary>
        /// If true, this specifies the time when the account went from a demo account to a regular one.
        /// </summary>
        DateTime? DemoConvertedOn { get; set; }

        /// <summary>
        /// Indicates when the trial period ends for the account.
        /// </summary>
        /// <remarks>Recurly doesn't allow for trials without a valid credit card.  So we implement a card-free trial situation.  If someone enters their card during the trial, a subscription is created and the trial plays out.</remarks>
        DateTime? TrialPeriodEndsOn { get; set; }

        /// <summary>
        /// If the user has a trial, this indicates the number of days left in the trial.  If the trial has expired, it will return 0 or negative numbers.
        /// </summary>
        int? DaysLeftInTrial { get; }

        /// <summary>
        /// Indicates whether the trial period hasn't ended yet.  Keep in mind their status could be Subscribed if this is true.
        /// </summary>
        bool InTrialPeriod { get; }

        /// <summary>
        /// The ID (ex: "solo") of the subscription plan this account has.
        /// </summary>
        string PlanId { get; set; }

        /// <summary>
        /// active, pastdue, expired
        /// </summary>
        AccountStatus Status { get; set; }

        /// <summary>
        /// Gets the logo for this account.  This logo becomes the "default" logo for productions.
        /// </summary>
        IImage Logo { get; }

        /// <summary>
        /// The default location for files underneath this account.  This defaults to a standard format but can be set explicitly.
        /// </summary>
        string FilesLocation { get; set; }

        /// <summary>
        /// Gets the number of videos this account currently has.  This does not include sample videos.
        /// </summary>
        /// <returns></returns>
        int VideoCount { get; }

        /// <summary>
        /// Gets the number of current non-sample productions this account currently has. Does not include samples.
        /// </summary>
        /// <returns></returns>
        int ProductionCount { get; }

        /// <summary>
        /// Gets the number of total bytes of asset files currently in storage. Does not include samples.
        /// </summary>
        /// <returns></returns>
        long AssetBytesTotal { get; }

        /// <summary>
        /// The total file size of all assets.
        /// </summary>
        FileSize AssetsTotalSize { get; }

        /// <summary>
        /// Gets the number of total asset files currently in the system.  Does not include samples.
        /// </summary>
        /// <returns></returns>
        int AssetFilesCount { get; }

        /// <summary>
        /// Gets the number of asset files that have ever been added.
        /// </summary>
        /// <returns></returns>
        int AssetFilesAllTimeCount { get; }

        IPlan Plan { get;  }
    }
}