using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Takeoff.Data;

namespace Takeoff.ViewModels
{
    public class Staff_Plans_Plan
    {
        public Staff_Plans_Plan()
        {
        }

        public Staff_Plans_Plan(IPlan data)
        {
            AllowSignups = data.AllowSignups;
            AssetFileSizeMax = data.AssetFileMaxSize;
            AssetsAllTimeMaxCount = data.AssetsAllTimeMaxCount;
            AssetsTotalMaxCount = data.AssetsTotalMaxCount;
            AssetsTotalSizeMax = data.AssetsTotalMaxSize;
            BillingIntervalLength = data.BillingIntervalLength;
            BillingIntervalUnit = data.BillingIntervalType.ToString();
            CreatedOn = data.CreatedOn;
            Title = data.Title;
            Notes = data.Notes;
            Id = data.Id;
            Price = data.PriceInCents/100.0;
            ProductionLimit = data.ProductionLimit;
            TrialIntervalLength = data.TrialIntervalLength;
            TrialIntervalUnit = data.TrialIntervalType.ToString();
            VideoFileSizeMax = data.VideoFileMaxSize;
            VideosTotalMaxCount = data.VideosTotalMaxCount;
            VideosPerBillingCycleMax = data.VideosPerBillingCycleMax;
            VideoDurationLimit = data.VideoDurationLimit;
        }

        [Required]
        [LocalizedDisplayName("AccountPlan_Model_Id")]
        public string Id { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_Title")]
        public string Title { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_Description")]
        public string Notes { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_BillingInterval")]
        public int BillingIntervalLength { get; set; }

        public string BillingIntervalUnit { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_TrialInterval")]
        public int TrialIntervalLength { get; set; }

        public string TrialIntervalUnit { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_VideosTotalMaxCount")]
        public int? VideosTotalMaxCount { get; set; }

        public int? VideosPerBillingCycleMax { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_VideoFileSizeMax")]
        public FileSize? VideoFileSizeMax { get; set; }

        public int? VideoDurationLimit { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_AssetsTotalSizeMax")]
        public FileSize? AssetsTotalSizeMax { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_AssetsTotalMaxCount")]
        public int? AssetsTotalMaxCount { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_AssetsAllTimeMaxCount")]
        public int? AssetsAllTimeMaxCount { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_AssetFileSizeMax")]
        public FileSize? AssetFileSizeMax { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_ProductionLimit")]
        public int? ProductionLimit { get; set; }

        [LocalizedDisplayName("AccountPlan_Model_AllowSignups")]
        public bool AllowSignups { get; set; }

        // [ModelBinder(typeof(CurrencyModelBinder))]
        [LocalizedDisplayName("AccountPlan_Model_Price")]
        public double Price { get; set; }

        public void UpdateData(IPlan data)
        {
            data.AllowSignups = AllowSignups;
            data.AssetFileMaxSize = AssetFileSizeMax;
            data.AssetsAllTimeMaxCount = AssetsAllTimeMaxCount;
            data.AssetsTotalMaxCount = AssetsTotalMaxCount;
            data.AssetsTotalMaxSize = AssetsTotalSizeMax;
            data.BillingIntervalLength = BillingIntervalLength;
            data.BillingIntervalType = Enum<PlanInterval>.Parse(BillingIntervalUnit, false);
            if (CreatedOn.HasValue)
                data.CreatedOn = CreatedOn;
            if (Id.HasChars() && data.Id != Id)
                data.Id = Id;
            data.Title = Title;
            data.Notes = Notes;
            data.PriceInCents = (int) (Price*100);
            data.ProductionLimit = ProductionLimit;
            data.TrialIntervalLength = TrialIntervalLength;
            data.TrialIntervalType = Enum<PlanInterval>.Parse(TrialIntervalUnit, false);
            data.VideoFileMaxSize = VideoFileSizeMax;
            data.VideosTotalMaxCount = VideosTotalMaxCount;
            data.VideosPerBillingCycleMax = VideosPerBillingCycleMax;
            data.VideoDurationLimit = VideoDurationLimit;
        }
    }
}