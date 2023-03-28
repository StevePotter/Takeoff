using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using System.IO;
using Takeoff.ViewModels;

namespace Takeoff.WebApp.Ripper
{
    class Plan: IPlan
    {
        public string Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public int? VideosTotalMaxCount { get; set; }
        public int? VideosPerBillingCycleMax { get; set; }
        public int? AssetsTotalMaxCount { get; set; }
        public int? AssetsAllTimeMaxCount { get; set; }
        public int? ProductionLimit { get; set; }
        public int? VideoDurationLimit { get; set; }
        public bool AllowSignups { get; set; }
        public int PriceInCents { get; set; }
        public bool IsFree { get; set; }
        public FileSize? VideoFileMaxSize { get; set; }
        public FileSize? AssetFileMaxSize { get; set; }
        public FileSize? AssetsTotalMaxSize { get; set; }
        public PlanInterval TrialIntervalType { get; set; }
        public int TrialIntervalLength { get; set; }
        public PlanInterval BillingIntervalType { get; set; }
        public int BillingIntervalLength { get; set; }

        #region Dummy Data

        public static IPlan Demo
        {
            get
            {
                return new Plan
                {
                    AllowSignups = false,
                    AssetFileMaxSize = FileSize.Parse("20gb"),
                    AssetsTotalMaxSize = FileSize.Parse("20tb"),
                    BillingIntervalType = PlanInterval.Months,
                    VideoFileMaxSize = FileSize.Parse("20gb"),
                    VideosTotalMaxCount = 20,
                    Title = "Freelance",
                    Id = "freelance",
                    TrialIntervalType = PlanInterval.Months,
                    PriceInCents = 10000,
                };
            }
        }

        public static IPlan Freelance
        {
            get
            {
                return new Plan
                           {
                               AllowSignups = true,
                               AssetFileMaxSize = FileSize.Parse("20gb"),
                               AssetsTotalMaxSize = FileSize.Parse("5gb"),
                               BillingIntervalType = PlanInterval.Months,
                               VideoFileMaxSize = FileSize.Parse("20gb"),
                               VideosTotalMaxCount = 10,
                               ProductionLimit = 5,
                               VideosPerBillingCycleMax = 20,
                               Title = "Freelance",
                               Id = "freelance",
                               TrialIntervalType = PlanInterval.Months,
                               PriceInCents = 10000,
                           };
            }
        }

        public static IPlan Studio
        {
            get
            {
                return new Plan
                {
                    AllowSignups = true,
                    AssetFileMaxSize = FileSize.Parse("20gb"),
                    AssetsTotalMaxSize = FileSize.Parse("15gb"),
                    BillingIntervalType = PlanInterval.Months,
                    VideoFileMaxSize = FileSize.Parse("20gb"),
                    VideosTotalMaxCount = 30,
                    ProductionLimit = 15,
                    VideosPerBillingCycleMax = 30,
                    Title = "Studio",
                    Id = "studio",
                    TrialIntervalType = PlanInterval.Months,
                    PriceInCents = 30000,
                };
            }
        }

        public static PlanForSale[] PlansForSale
        {
            get
            {
                return new PlanForSale[] { new PlanForSale(Plan.Freelance), new PlanForSale(Plan.Studio), };
            }
        }

        #endregion
    }


}
