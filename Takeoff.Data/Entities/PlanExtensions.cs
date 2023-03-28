using System;
using System.IO;
using System.Linq;
using Takeoff.Models;

namespace Takeoff.Data
{
    public static class PlanIds
    {
        /// <summary>
        /// Plan ID for account status TrialAnonymous.
        /// </summary>
        public const string TrialAnonymous = "trial_anonymous";
        /// <summary>
        /// Plan ID for account status Trial2.
        /// </summary>
        public const string Trial = "trial_known";

        public const string Solo = "solo";
        public const string Studio = "studio";

    }

    /// <summary>
    /// Helper extensions for the IPlan interface.
    /// </summary>
    public static class PlanExtensions
    {
        /// <summary>
        /// Copies all properties from this (source) to the target passed.  Source will remain untouched.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static void CopyTo(this IPlan source, IPlan target)
        {
            target.AllowSignups = source.AllowSignups;
            target.AssetFileMaxSize = source.AssetFileMaxSize;
            target.AssetsAllTimeMaxCount = source.AssetsAllTimeMaxCount;
            target.AssetsTotalMaxCount = source.AssetsTotalMaxCount;
            target.AssetsTotalMaxSize = source.AssetsTotalMaxSize;
            target.BillingIntervalLength = source.BillingIntervalLength;
            target.BillingIntervalType = source.BillingIntervalType;
            target.CreatedOn = source.CreatedOn;
            target.Id = source.Id;
            target.Title = source.Title;
            target.Notes = source.Notes;
            target.PriceInCents = source.PriceInCents;
            target.ProductionLimit = source.ProductionLimit;
            target.TrialIntervalLength = source.TrialIntervalLength;
            target.TrialIntervalType = source.TrialIntervalType;
            target.VideoFileMaxSize = source.VideoFileMaxSize;
            target.VideosTotalMaxCount = source.VideosTotalMaxCount;
            target.VideosPerBillingCycleMax = source.VideosPerBillingCycleMax;
        }

        public static double PriceInDollars(this IPlan plan)
        {
            return (double)plan.PriceInCents / 100.0;
        }

        public static DateTime BillingCycleEnd(this IPlan plan, DateTime start)
        {
            if ( !plan.BillingIntervalLength.IsPositive())
                return start;

            return plan.BillingIntervalType == PlanInterval.Days
                       ? start.AddDays(plan.BillingIntervalLength)
                       : start.AddMonths(plan.BillingIntervalLength);
        }

    }
}