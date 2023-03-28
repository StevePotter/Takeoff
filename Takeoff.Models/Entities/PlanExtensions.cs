using System;
using System.IO;
using System.Linq;
using Takeoff.Models;

namespace Takeoff.Data
{

    /// <summary>
    /// Helper extensions for the IPlan interface.
    /// </summary>
    public static class PlanExtensions
    {
        //public static bool IsFree(this IPlan plan)
        //{
        //    return plan.PriceInCents <= 0;
        //}

        //public static FileSize? VideoFileSizeMax(this IPlan plan)
        //{
        //    var size = plan.VideoFileSizeMaxBytes;
        //    if (!size.HasValue)
        //            return new FileSize?();
        //    return new FileSize(size.Value);
        //}

        //public static FileSize? AssetFileSizeMax(this IPlan plan)
        //{
        //    var size = plan.AssetFileSizeMaxBytes;
        //    if (!size.HasValue)
        //            return new FileSize?();
        //    return new FileSize(size.Value);
        //}

        //public static FileSize? AssetsTotalMax(this IPlan plan)
        //{            
        //                var size = plan.AssetsTotalMaxBytes;
        //    if (!size.HasValue)
        //            return new FileSize?();
        //    return new FileSize(size.Value);
        //}


        //public static PlanInterval TrialIntervalType(this IPlan plan)
        //{
        //    return (PlanInterval)Enum.Parse(typeof(PlanInterval), plan.TrialIntervalUnit, true);
        //}

        //public static PlanInterval BillingIntervalType(this IPlan plan)
        //{
        //    return (PlanInterval)Enum.Parse(typeof(PlanInterval), plan.BillingIntervalUnit, true);
        //}

        /// <summary>
        /// Copies all properties from this (source) to the target passed.  Source will remain untouched.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static void CopyTo(this IPlan source, IPlan target)
        {
            target.AllowSignups = source.AllowSignups;
            target.AssetFileSizeMaxBytes = source.AssetFileSizeMax.HasValue ? source.AssetFileSizeMax.Value.Bytes : new long?();
            target.AssetsAllTimeMaxCount = source.AssetsAllTimeMaxCount;
            target.AssetsTotalMaxCount = source.AssetsTotalMaxCount;
            target.AssetsTotalMaxBytes = source.AssetsTotalMax.HasValue ? source.AssetsTotalMax.Value.Bytes : new long?();
            target.BillingIntervalLength = source.BillingIntervalLength;
            target.BillingIntervalUnit = source.BillingIntervalUnit;
            if (source.CreatedOn.HasValue)
                target.CreatedOn = source.CreatedOn;
            if (source.Id.HasChars() && target.Id != source.Id)
                target.Id = source.Id;
            target.Title = source.Title;
            target.Notes = source.Notes;
            target.PriceInCents = source.PriceInCents;
            target.ProductionLimit = source.ProductionLimit;
            target.TrialIntervalLength = source.TrialIntervalLength;
            target.TrialIntervalUnit = source.TrialIntervalUnit;
            target.VideoFileSizeMaxBytes = source.VideoFileSizeMax.HasValue ? source.VideoFileSizeMax.Value.Bytes : new long?();
            target.VideosTotalMaxCount = source.VideosTotalMaxCount;
        }

        public static double PriceInDollars(this IPlan plan)
        {
            return (double)plan.PriceInCents / 100.0;
        }

    }
}