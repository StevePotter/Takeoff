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
        /// If set, this specifies the time when the account went from a demo account to a regular one.  If not set, they are either still in demo mode or didn't come from the demo.
        /// </summary>
        DateTime? DemoConvertedOn { get; set; }

        /// <summary>
        /// If they are signed up, this indicates when the current billing period began, which happens when they sign up for a trial (don't subscribe), sign up with subscribing, or their billing is renewed.  
        /// </summary>
        DateTime? CurrentBillingPeriodStartedOn { get; set; }

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
        /// Gets the number of videos this account currently has.  This does not include sample videos but does include beta videos.
        /// </summary>
        /// <returns></returns>
        int VideoCount { get; }

        /// <summary>
        /// Gets the number of videos that count toward account plan limits.
        /// </summary>
        /// <returns></returns>
        int VideoCountBillable { get; }

        /// <summary>
        /// Gets the number of vidoes that have been uploaded since the billing cycle began.  This includes videos that have been deleted, but doesn't include videos that failed processing.
        /// </summary>
        int VideosAddedInBillingCycle { get;  }

        /// <summary>
        /// Gets the number of current non-sample productions this account currently has. Does not include samples but does include beta productions.
        /// </summary>
        /// <returns></returns>
        int ProductionCount { get; }

        /// <summary>
        /// Gets the number of productions that count toward account plan limits.
        /// </summary>
        /// <returns></returns>
        int ProductionCountBillable { get; }

        /// <summary>
        /// The total file size of all assets.
        /// </summary>
        FileSize AssetsTotalSize { get; }

        /// <summary>
        /// The total size of all assets that count toward the user's plan.
        /// </summary>
        FileSize AssetsTotalSizeBillable { get; set; }

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

        /// <summary>
        /// The plan this account has.  This determines billing as well as limits.
        /// </summary>
        IPlan Plan { get; }

        /// <summary>
        /// When set, indicates exactly when this account was converted from a beta account to a paid one.
        /// </summary>
        DateTime? ConvertedFromBetaOn { get; set; }

        /// <summary>
        /// Indicates whether the account has explicit access to a certain beta feature.
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        bool HasAccessToFeature(string feature);

    }
}