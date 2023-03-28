using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Takeoff.Data;

namespace Takeoff.WebApp.Ripper
{
    class Account: IAccount
    {
        public int? LogoImageId
        {
            get; set;
        }

        public DateTime? DemoConvertedOn
        {
            get;
            set;
        }

        public DateTime? TrialPeriodEndsOn
        {
            get;
            set;
        }

        public int? DaysLeftInTrial
        {
            get;
            set;

        }

        public bool InTrialPeriod
        {
            get;
            set;

        }

        public string PlanId
        {
            get;
            set;

        }

        public AccountStatus Status
        {
            get;
            set;
        }

        public IImage Logo
        {
            get;
            set;

        }

        public string FilesLocation
        {
            get;
            set;

        }

        public int VideoCount
        {
            get;
            set;

        }

        public int VideoCountBillable { get; set; }
        public int VideosAddedInBillingCycle { get; set; }

        public int ProductionCount
        {
            get;
            set;

        }

        public int ProductionCountBillable { get; set; }


        public System.IO.FileSize AssetsTotalSize
        {
            get;
            set;

        }

        public DateTime? CurrentBillingPeriodStartedOn { get; set; }

        public System.IO.FileSize AssetsTotalSizeBillable { get; set; }


        public int AssetFilesCount
        {
            get;
            set;

        }

        public int AssetFilesAllTimeCount
        {
            get;
            set;

        }

        public IPlan Plan
        {
            get;
            set;

        }

        public DateTime? ConvertedFromBetaOn
        {
            get;
            set;

        }

        public int AccountId
        {
            get;
            set;

        }

        public int Id
        {
            get;
            set;

        }

        public int CreatedByUserId
        {
            get;
            set;

        }

        public int OwnerUserId
        {
            get;
            set;

        }

        public DateTime CreatedOn
        {
            get;
            set;

        }

        public int? LastChangeId
        {
            get;
            set;

        }

        public DateTime? LastChangeDate
        {
            get;
            set;

        }

        public bool HasAccessToFeature(string feature)
        {
            return true;
        }

        #region Dummy Data

        public static IAccount SubscribedFreelance
        {
            get
            {
                return new Account
                {
                    Status = AccountStatus.Subscribed,
                    Plan = Ripper.Plan.Freelance,
                    AccountId = 0,
                    AssetFilesAllTimeCount = 1201,
                    AssetFilesCount = 78,
                    AssetsTotalSize = new FileSize("1.45gb"),
                    AssetsTotalSizeBillable = new FileSize("0.45gb"),
                    ConvertedFromBetaOn = DateTime.Parse("11/1/2011").ToUniversalTime(),
                    ProductionCount = 2,
                    ProductionCountBillable = 2,
                    VideoCount = 120,
                    VideoCountBillable = 4,
                    VideosAddedInBillingCycle = 3,
                };                
            }
        }

        public static IAccount PastDueFreelance
        {
            get
            {
                return SubscribedFreelance.CloneProperties().SetProperty(m => m.Status, AccountStatus.Pastdue);
            }
        }


        public static IAccount Demo
        {
            get
            {
                return new Account
                {
                    Status = AccountStatus.Demo,
                    Plan = Ripper.Plan.Demo,
                };
            }
        }

        public static IAccount Trial2
        {
            get
            {
                return new Account
                {
                    Status = AccountStatus.Trial2,
                    Plan = Ripper.Plan.Demo,
                };
            }
        }

        /// <summary>
        /// A account on the trial, subscribed to freelance.
        /// </summary>
        public static IAccount TrialFreelance
        {
            get
            {
                return new Account
                {
                    Status = AccountStatus.Trial,
                    Plan = Ripper.Plan.Freelance,
                    TrialPeriodEndsOn = DateTime.Now.AddDays(10),
                    AccountId = 0,
                    AssetFilesAllTimeCount = 1201,
                    AssetFilesCount = 78,
                    AssetsTotalSize = new FileSize("1.45gb"),
                    AssetsTotalSizeBillable = new FileSize("0.45gb"),
                    ConvertedFromBetaOn = null,
                    ProductionCount = 2,
                    ProductionCountBillable = 2,
                    VideoCount = 120,
                    VideoCountBillable = 4,
                    VideosAddedInBillingCycle = 3,
                };
            }
        }

        /// <summary>
        /// A beta account on the trial, subscribed to freelance.
        /// </summary>
        public static IAccount TrialFreelanceBeta
        {
            get
            {
                return TrialFreelance.CloneProperties().SetProperty(m => m.ConvertedFromBetaOn, DateTime.Parse("11/1/2011").ToUniversalTime());
            }
        }


        /// <summary>
        /// A account on the trial, subscribed to freelance.
        /// </summary>
        public static IAccount TrialExpired
        {
            get
            {
                return new Account
                {
                    Status = AccountStatus.TrialExpired,
                    Plan = Ripper.Plan.Freelance,
                    TrialPeriodEndsOn = DateTime.Now.AddDays(-1),
                    AccountId = 0,
                    AssetFilesAllTimeCount = 1201,
                    AssetFilesCount = 78,
                    AssetsTotalSize = new FileSize("1.45gb"),
                    AssetsTotalSizeBillable = new FileSize("0.45gb"),
                    ConvertedFromBetaOn = null,
                    ProductionCount = 2,
                    ProductionCountBillable = 2,
                    VideoCount = 120,
                    VideoCountBillable = 4,
                    VideosAddedInBillingCycle = 3,
                };
            }
        }

        /// <summary>
        /// A beta account on the trial, subscribed to freelance.
        /// </summary>
        public static IAccount TrialExpiredBeta
        {
            get
            {
                return TrialExpired.CloneProperties().SetProperty(m => m.ConvertedFromBetaOn, DateTime.Parse("11/1/2011").ToUniversalTime());
            }
        }


        #endregion




    }
}
