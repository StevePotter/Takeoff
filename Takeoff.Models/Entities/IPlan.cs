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

        int BillingIntervalLength { get; set; }

        string BillingIntervalUnit { get; set; }

        int TrialIntervalLength { get; set; }

        string TrialIntervalUnit { get; set; }

        int? VideosTotalMaxCount { get; set; }

        long? VideoFileSizeMaxBytes { get; set; }

        long? AssetsTotalMaxBytes { get; set; }

        int? AssetsTotalMaxCount { get; set; }

        int? AssetsAllTimeMaxCount { get; set; }

        long? AssetFileSizeMaxBytes { get; set; }

        int? ProductionLimit { get; set; }

        bool AllowSignups { get; set; }

        int PriceInCents { get; set; }

        bool IsFree { get; }

        FileSize? VideoFileSizeMax { get; }

        FileSize? AssetFileSizeMax { get; }
        
        FileSize? AssetsTotalMax { get; }
        
        PlanInterval TrialIntervalType { get; }
        
        PlanInterval BillingIntervalType { get; }
        
        double PriceInDollars { get; }

    }
}