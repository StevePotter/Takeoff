using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;
using System.Text;
using System.Dynamic;
using System.Globalization;
using Recurly;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Runtime.Serialization;

namespace Takeoff.Models
{

    [ThingType("Account")]
    [Serializable]
    public class AccountThing : ThingBase, IAccount, IContainerThing
    {

        #region Constructors

        public AccountThing()
        {
        }

        protected AccountThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
      
        #region Properties


        /// <summary>
        /// When set, indicates the ID of the direct child ImageThing that represents a logo shown next to the title.
        /// </summary>
        public int? LogoImageId
        {
            get
            {
                return LogoImageIdProperty.GetValue(this);
            }
            set
            {
                LogoImageIdProperty.SetValue(this, value);
            }
        }
        private int? _LogoImageId;
        private static readonly ThingDataProperty<int?, AccountThing, Data.Account> LogoImageIdProperty = new ThingDataProperty<int?, AccountThing, Data.Account>()
        {
            Name = "LogoImageId",
            SetField = (o, v) => o._LogoImageId = v,
            GetField = o => o._LogoImageId,
            ShouldGetData = r => r.LogoImageId != null,
            GetData = o => o.LogoImageId,
            SetData = (o, v) => o.LogoImageId = v,
        }.Register();
	

        /// <summary>
        /// If true, this specifies the time when the account went from a demo account to a regular one.
        /// </summary>
        public DateTime? DemoConvertedOn
        {
            get
            {
                return DemoConvertedOnProperty.GetValue(this);
            }
            set
            {
                DemoConvertedOnProperty.SetValue(this, value);
            }
        }
        private DateTime? _DemoConvertedOn;
        private static readonly ThingDataProperty<DateTime?, AccountThing, Data.Account> DemoConvertedOnProperty = new ThingDataProperty<DateTime?, AccountThing, Data.Account>()
        {
            Name = "DemoConvertedOn",
            SetField = (o, v) => o._DemoConvertedOn = v,
            GetField = o => o._DemoConvertedOn,
            ShouldGetData = r => r.DemoConvertedOn != null,
            GetData = o => o.DemoConvertedOn,
            SetData = (o, v) => o.DemoConvertedOn = v,
        }.Register();
	

        /// <summary>
        /// Indicates when the trial period ends for the account.
        /// </summary>
        /// <remarks>Recurly doesn't allow for trials without a valid credit card.  So we implement a card-free trial situation.  If someone enters their card during the trial, a subscription is created and the trial plays out.</remarks>
        public DateTime? TrialPeriodEndsOn
        {
            get
            {
                return TrialPeriodEndsOnProperty.GetValue(this);
            }
            set
            {
                TrialPeriodEndsOnProperty.SetValue(this, value);
            }
        }
        private DateTime? _TrialPeriodEndsOn;
        private static readonly ThingDataProperty<DateTime?, AccountThing, Data.Account> TrialPeriodEndsOnProperty = new ThingDataProperty<DateTime?, AccountThing, Data.Account>()
        {
            Name = "TrialPeriodEndsOn",
            SetField = (o, v) => o._TrialPeriodEndsOn = v,
            GetField = o => o._TrialPeriodEndsOn,
            ShouldGetData = r => r.TrialPeriodEndsOn != null,
            GetData = o => o.TrialPeriodEndsOn,
            SetData = (o, v) => o.TrialPeriodEndsOn = v,
        }.Register();
	

        /// <summary>
        /// If the user has a trial, this indicates the number of days left in the trial.  If the trial has expired, it will return 0 or negative numbers.
        /// </summary>
        public int? DaysLeftInTrial
        {
            get
            {
                var period = TrialPeriodEndsOn;
                if (!period.HasValue)
                    return new int?();

                return (int)(period.Value.Date - DateTime.Today).TotalDays;
            }
        }

        /// <summary>
        /// The ID (ex: "solo") of the subscription plan this account has.
        /// </summary>
        public string PlanId
        {
            get
            {
                return PlanIdProperty.GetValue(this);
            }
            set
            {
                PlanIdProperty.SetValue(this, value);
            }
        }
        private string _PlanId;
        private static readonly ThingDataProperty<string, AccountThing, Data.Account> PlanIdProperty = new ThingDataProperty<string, AccountThing, Data.Account>()
        {
            Name = "PlanId",
            SetField = (o, v) => o._PlanId = v,
            GetField = o => o._PlanId,
            GetData = o => o.PlanId,
            SetData = (o, v) => o.PlanId = v,
        }.Register();



        /// <summary>
        /// Indicates whether the account was created during the Takeoff beta period.
        /// </summary>
        public DateTime? ConvertedFromBetaOn
        {
            get
            {
                return ConvertedFromBetaOnProperty.GetValue(this);
            }
            set
            {
                ConvertedFromBetaOnProperty.SetValue(this, value);
            }
        }
        private DateTime? _ConvertedFromBetaOn;
        private static readonly ThingDataProperty<DateTime?, AccountThing, Data.Account> ConvertedFromBetaOnProperty = new ThingDataProperty<DateTime?, AccountThing, Data.Account>()
        {
            Name = "ConvertedFromBetaOn",
            SetField = (o, v) => o._ConvertedFromBetaOn = v,
            GetField = o => o._ConvertedFromBetaOn,
            GetData = o => o.ConvertedFromBetaOn,
            SetData = (o, v) => o.ConvertedFromBetaOn = v,
        }.Register();
	

        public IPlan Plan
        {
            get
            {
                if (!PlanId.HasChars())
                    return null;                
                return IoC.Get<IPlansRepository>().Get(PlanId);
            }
        }


        /// <summary>
        /// When true, there is a problem such as billing problem or abuse, and the account has been suspended.
        /// </summary>
        public bool IsSuspended
        {
            get
            {
                switch( Status )
                {
                    case AccountStatus.ExpiredNonpayment:
                    case AccountStatus.Pastdue:
                    case AccountStatus.TrialExpired:
                    case AccountStatus.Suspended:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// active, pastdue, expired
        /// </summary>
        public AccountStatus Status
        {
            get
            {
                return StatusProperty.GetValue(this);
            }
            set
            {
                StatusProperty.SetValue(this, value);
            }
        }
        private AccountStatus _Status;
        private static readonly ThingDataProperty<AccountStatus, AccountThing, Data.Account> StatusProperty = new ThingDataProperty<AccountStatus, AccountThing, Data.Account>()
        {
            Name = "Subscription",
            SetField = (o, v) => o._Status = v,
            GetField = o => o._Status,
            GetData = o => o.Status == null ? AccountStatus.Suspended : (AccountStatus)Enum.Parse(typeof(AccountStatus),o.Status),//if data is null, the account is invalid
            SetData = (o, v) => o.Status = v.ToString(),
            FromJson = v => (AccountStatus)Convert.ToInt32(v),//note that if you were to use anything besides .net here, you'd want to convert to string
            ToJson = v => (int)v,
            ToJsonSimple2 = v => (int)v,
        }.Register();


        /// <summary>
        /// Gets the logo for this account.  This logo becomes the "default" logo for productions.
        /// </summary>
        public ImageThing Logo
        {
            get
            {
                if (LogoImageId.HasValue)
                {
                    return (ImageThing)FindDescendantById(LogoImageId.Value);
                }

                return null;
            }
        }

        /// <summary>
        /// The default location for files underneath this account.  This defaults to a standard format but can be set explicitly.
        /// </summary>
        public string FilesLocation
        {
            get
            {
                return FilesLocationProperty.GetValue(this);
            }
            set
            {
                FilesLocationProperty.SetValue(this, value);
            }
        }
        private string _FilesLocation;
        private static readonly ThingDataProperty<string, AccountThing, Data.Account> FilesLocationProperty = new ThingDataProperty<string, AccountThing, Data.Account>()
        {
            Name = "FilesLocation",
            SetField = (o, v) => o._FilesLocation = v,
            GetField = o => o._FilesLocation,
            DefaultValueCallback = o => ConfigUtil.GetRequiredAppSetting("ProductionBucket").EndWith("/") + o.Id.ToInvariant(),
            GetData = o => o.FilesLocation,
            SetData = (o, v) => o.FilesLocation = v,
        }.Register();


        /// <summary>
        /// Indicates the date when the current billing period began.  This is used to enforce limits on number of video uploads in a month.
        /// </summary>
        public DateTime? CurrentBillingPeriodStartedOn
        {
            get
            {
                return CurrentBillingPeriodStartedOnProperty.GetValue(this);
            }
            set
            {
                CurrentBillingPeriodStartedOnProperty.SetValue(this, value);
            }
        }
        private DateTime? _CurrentBillingPeriodStartedOn;
        private static readonly ThingDataProperty<DateTime?, AccountThing, Data.Account> CurrentBillingPeriodStartedOnProperty = new ThingDataProperty<DateTime?, AccountThing, Data.Account>()
        {
            Name = "CurrentBillingPeriodStartedOn",
            SetField = (o, v) => o._CurrentBillingPeriodStartedOn = v,
            GetField = o => o._CurrentBillingPeriodStartedOn,
            ShouldGetData = o => o.CurrentBillingPeriodStartedOn != null,
            GetData = o => o.CurrentBillingPeriodStartedOn,
            SetData = (o, v) => o.CurrentBillingPeriodStartedOn = v,
        }.Register();
	

        /// <summary>
        /// Gets the number of videos this account currently has.  This does not include sample videos.
        /// </summary>
        /// <returns></returns>
        public int VideoCount
        {
            get
            {
                return VideoCountProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                VideoCountProperty.SetValue(this, value);
            }
        }
        private int _VideoCount;
        private static readonly ThingProperty<int, AccountThing> VideoCountProperty = new ThingProperty<int, AccountThing>()
        {
            Name = "VideoCount",
            SetField = (o, v) => o._VideoCount = v,
            GetField = o => o._VideoCount,
        }.Register();


        public int VideoCountBillable
        {
            get
            {
                return VideoCountBillableProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                VideoCountBillableProperty.SetValue(this, value);
            }
        }
        private int _VideoCountBillable;
        private static readonly ThingProperty<int, AccountThing> VideoCountBillableProperty = new ThingProperty<int, AccountThing>()
        {
            Name = "VideoCountBillable",
            SetField = (o, v) => o._VideoCountBillable = v,
            GetField = o => o._VideoCountBillable,
        }.Register();


        public int VideosAddedInBillingCycle
        {
            get
            {
                return VideosAddedInBillingCycleProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                VideosAddedInBillingCycleProperty.SetValue(this, value);
            }
        }
        private int _VideosAddedInBillingCycle;
        private static readonly ThingProperty<int, AccountThing> VideosAddedInBillingCycleProperty = new ThingProperty<int, AccountThing>()
        {
            Name = "VideosAddedInBillingCycle",
            SetField = (o, v) => o._VideosAddedInBillingCycle = v,
            GetField = o => o._VideosAddedInBillingCycle,
        }.Register();


        /// <summary>
        /// Gets the number of current non-sample productions this account currently has. Does not include samples.
        /// </summary>
        /// <returns></returns>
        public int ProductionCount
        {
            get
            {
                return ProductionCountProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                ProductionCountProperty.SetValue(this, value);
            }
        }
        private int _ProductionCount;
        private static readonly ThingProperty<int, AccountThing> ProductionCountProperty = new ThingProperty<int, AccountThing>()
        {
            Name = "ProductionCount",
            SetField = (o, v) => o._ProductionCount = v,
            GetField = o => o._ProductionCount,
        }.Register();

        
        /// <summary>
        /// Gets the number of current non-sample productions this account currently has. Does not include samples.
        /// </summary>
        /// <returns></returns>
        public int ProductionCountBillable
        {
            get
            {
                return ProductionCountBillableProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                ProductionCountBillableProperty.SetValue(this, value);
            }
        }
        private int _ProductionCountBillable;
        private static readonly ThingProperty<int, AccountThing> ProductionCountBillableProperty = new ThingProperty<int, AccountThing>()
        {
            Name = "ProductionCountBillable",
            SetField = (o, v) => o._ProductionCountBillable = v,
            GetField = o => o._ProductionCountBillable,
        }.Register();

        /// <summary>
        /// Gets the number of total bytes of asset files currently in storage. Does not include samples.
        /// </summary>
        /// <returns></returns>
        public long AssetBytesTotal
        {
            get
            {
                return AssetBytesTotalProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                AssetBytesTotalProperty.SetValue(this, value);
            }
        }
        private long _AssetBytesTotal;
        private static readonly ThingProperty<long, AccountThing> AssetBytesTotalProperty = new ThingProperty<long, AccountThing>()
        {
            Name = "AssetBytesTotal",
            SetField = (o, v) => o._AssetBytesTotal = v,
            GetField = o => o._AssetBytesTotal,
        }.Register();


        public FileSize AssetsTotalSizeBillable
        {
            get
            {
                return AssetsTotalSizeBillableProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                AssetsTotalSizeBillableProperty.SetValue(this, value);
            }
        }
        private FileSize _AssetsTotalSizeBillable;
        private static readonly ThingProperty<FileSize, AccountThing> AssetsTotalSizeBillableProperty = new ThingProperty<FileSize, AccountThing>()
        {
            Name = "AssetsTotalSizeBillable",
            SetField = (o, v) => o._AssetsTotalSizeBillable = v,
            GetField = o => o._AssetsTotalSizeBillable,
            ToJson = (v) => v.Bytes,
            ToJsonSimple2 = v => v.Bytes,
            FromJson = (o) => new FileSize(Convert.ToInt64(o)),
        }.Register();

        /// <summary>
        /// The total file size of all assets.
        /// </summary>
        public FileSize AssetsTotalSize
        {
            get
            {
                return new FileSize(AssetBytesTotal);
            }
        }

        /// <summary>
        /// Gets the number of total asset files currently in the system.  Does not include samples.
        /// </summary>
        /// <returns></returns>
        public int AssetFilesCount
        {
            get
            {
                return AssetFilesCountProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                AssetFilesCountProperty.SetValue(this, value);
            }
        }
        private int _AssetFilesCount;
        private static readonly ThingProperty<int, AccountThing> AssetFilesCountProperty = new ThingProperty<int, AccountThing>()
        {
            Name = "AssetFilesCount",
            SetField = (o, v) => o._AssetFilesCount = v,
            GetField = o => o._AssetFilesCount,
        }.Register();

        /// <summary>
        /// Gets the number of asset files that have ever been added.
        /// </summary>
        /// <returns></returns>
        public int AssetFilesAllTimeCount
        {
            get
            {
                return AssetFilesAllTimeCountProperty.GetValue(this);
            }
            set //should only be set by data access code and computed automatically
            {
                AssetFilesAllTimeCountProperty.SetValue(this, value);
            }
        }
        private int _AssetFilesAllTimeCount;
        private static readonly ThingProperty<int, AccountThing> AssetFilesAllTimeCountProperty = new ThingProperty<int, AccountThing>()
        {
            Name = "AssetFilesAllTimeCount",
            SetField = (o, v) => o._AssetFilesAllTimeCount = v,
            GetField = o => o._AssetFilesAllTimeCount,
        }.Register();


        protected HashSet<string> FeaturesWithAccess
        {
            get { return _FeaturesWithAccess ?? (_FeaturesWithAccess = new HashSet<string>()); }
        }
        private HashSet<string> _FeaturesWithAccess;

        #endregion


        protected override void AddToCacheOverride()
        {
            base.AddToCacheOverride();
        }

        protected override int GetDefaultAccountId()
        {
            return this.Id;
        }

        public bool HasAccessToFeature(string feature)
        {
            return _FeaturesWithAccess != null && _FeaturesWithAccess.Contains(feature);
        }

        public IEnumerable<AccountMembershipThing> GetMembers()
        {
            return this.Children.OfType<AccountMembershipThing>();
        }

        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;

            yield return this.CreateAuxDataFiller<Data.Account>(db);

            yield return new ThingAuxDataFiller<AccountComputedData>((from zzz in db.Things
                                                        select new AccountComputedData
                                                          {
                                                              VideoCount = (from vt in db.Things
                                                                            join v in db.Videos on vt.Id equals v.ThingId
                                                                            where vt.AccountId == Id && vt.DeletedOn == null && v.Duration != null && (v.IsSample == null || !(bool)v.IsSample)
                                                                            select vt).Count(),
                                                              VideoCountBillable = (from vt in db.Things
                                                                            join v in db.Videos on vt.Id equals v.ThingId
                                                                            where vt.AccountId == Id && vt.DeletedOn == null && v.Duration != null && (v.IsComplimentary == null || !(bool)v.IsComplimentary)
                                                                            select vt).Count(),
                                                              VideosAddedInBillingCycle = (from vt in db.Things
                                                                                           join v in db.Videos on vt.Id equals v.ThingId
                                                                                           join a in db.Accounts on vt.AccountId equals a.ThingId
                                                                                           where vt.AccountId == Id && a.CurrentBillingPeriodStartedOn != null && v.Duration != null && vt.CreatedOn >= (DateTime)a.CurrentBillingPeriodStartedOn && (v.IsComplimentary == null || !(bool)v.IsComplimentary)//includes deleted ones as well
                                                                                           select vt).Count(), 
                                                              ProductionCount = (from t in db.Things
                                                                  join p in db.Projects on t.Id equals p.ThingId
                                                                  where t.AccountId == Id && t.DeletedOn == null && (p.IsSample == null || !(bool)p.IsSample)
                                                                  select t).Count(),
                                                              ProductionCountBillable = (from t in db.Things
                                                                  join p in db.Projects on t.Id equals p.ThingId
                                                                  where t.AccountId == Id && t.DeletedOn == null && (p.IsComplimentary == null || !(bool)p.IsComplimentary)
                                                                  select t).Count(),
                                                              AssetBytesTotal = (from ft in db.Things
                                                                   join f in db.Files on ft.Id equals f.ThingId
                                                                   where ft.AccountId == Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsSample == null || !(bool)f.IsSample)
                                                                   select (long?)f.Bytes).Sum().GetValueOrDefault(),
                                                              AssetBytesTotalBillable = (from ft in db.Things
                                                                   join f in db.Files on ft.Id equals f.ThingId
                                                                   where ft.AccountId == Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsComplimentary == null || !(bool)f.IsComplimentary)
                                                                   select (long?)f.Bytes).Sum().GetValueOrDefault(),
                                                              AssetFilesCount = (from ft in db.Things
                                                                                 join f in db.Files on ft.Id equals f.ThingId
                                                                                 where ft.AccountId == Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsSample == null || !(bool)f.IsSample)
                                                                                 select f).Count(),
                                                              AssetFilesAllTimeCount = (from ft in db.FileUploadLogs where ft.AccountId == Id && ft.FileThingType == Things.ThingType(typeof(FileThing)) select ft).Count()
                                                          }).Take(1), t =>
                                                             {
                                                                 this.VideoCount = t.VideoCount;
                                                                 this.VideoCountBillable = t.VideoCountBillable;
                                                                 this.VideosAddedInBillingCycle =
                                                                     t.VideosAddedInBillingCycle;
                                                                 this.ProductionCount = t.ProductionCount;
                                                                 this.ProductionCountBillable = t.ProductionCountBillable;
                                                                 this.AssetBytesTotal = t.AssetBytesTotal;
                                                                 this.AssetsTotalSizeBillable = new FileSize(t.AssetBytesTotalBillable);
                                                                 this.AssetFilesCount = t.AssetFilesCount;
                                                                 this.AssetFilesAllTimeCount = t.AssetFilesAllTimeCount;
                                                             });

        }

        public override void AddDataFillers(List<IThingAuxDataFiller> fillers, Dictionary<int, ThingBase> things, DataModel db)
        {
            base.AddDataFillers(fillers, things, db);

            (from data in db.Accounts
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
            ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.AccountMemberships
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
            ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.FeatureAccesses
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Images
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Files
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Memberships
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (
                    from account in db.Accounts
                    join accountThing in db.Things on account.ThingId equals accountThing.Id
                    where accountThing.DeletedOn == null && accountThing.Id == Id
                    select new AccountComputedData
                    {
                        AccountId = accountThing.Id,
                        VideoCount = (from vt in db.Things
                                      join v in db.Videos on vt.Id equals v.ThingId
                                      where vt.AccountId == accountThing.Id && vt.DeletedOn == null && v.Duration != null && (v.IsSample == null || !(bool)v.IsSample)
                                      select vt).Count(),
                        VideoCountBillable = (from vt in db.Things
                                              join v in db.Videos on vt.Id equals v.ThingId
                                              where vt.AccountId == accountThing.Id && vt.DeletedOn == null && v.Duration != null && (v.IsComplimentary == null || !(bool)v.IsComplimentary)
                                              select vt).Count(),
                        VideosAddedInBillingCycle = (from vt in db.Things
                                                     join v in db.Videos on vt.Id equals v.ThingId
                                                     join a in db.Accounts on vt.AccountId equals a.ThingId
                                                     where vt.AccountId == accountThing.Id && a.CurrentBillingPeriodStartedOn != null && v.Duration != null && vt.CreatedOn >= (DateTime)a.CurrentBillingPeriodStartedOn && (v.IsComplimentary == null || !(bool)v.IsComplimentary)//includes deleted ones as well
                                                     select vt).Count(),
                        ProductionCount = (from t in db.Things
                                           join p in db.Projects on t.Id equals p.ThingId
                                           where t.AccountId == accountThing.Id && t.DeletedOn == null && (p.IsSample == null || !(bool)p.IsSample)
                                           select t).Count(),
                        ProductionCountBillable = (from t in db.Things
                                                   join p in db.Projects on t.Id equals p.ThingId
                                                   where t.AccountId == accountThing.Id && t.DeletedOn == null && (p.IsComplimentary == null || !(bool)p.IsComplimentary)
                                                   select t).Count(),
                        AssetBytesTotal = (from ft in db.Things
                                           join f in db.Files on ft.Id equals f.ThingId
                                           join p in db.Projects on ft.ParentId equals p.ThingId//needed because source video files are not included in assets.  Assets are children of production, where source videos are children of videos.  this fixes that, as ghetto as it may be
                                           where ft.AccountId == accountThing.Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsSample == null || !(bool)f.IsSample)
                                           select (long?)f.Bytes).Sum().GetValueOrDefault(),
                        AssetBytesTotalBillable = (from ft in db.Things
                                                   join f in db.Files on ft.Id equals f.ThingId
                                                   join p in db.Projects on ft.ParentId equals p.ThingId//needed because source video files are not included in assets.  Assets are children of production, where source videos are children of videos.  this fixes that, as ghetto as it may be
                                                   where ft.AccountId == accountThing.Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsComplimentary == null || !(bool)f.IsComplimentary)
                                                   select (long?)f.Bytes).Sum().GetValueOrDefault(),

                        AssetFilesCount = (from ft in db.Things
                                           join f in db.Files on ft.Id equals f.ThingId
                                           where ft.AccountId == accountThing.Id && ft.DeletedOn == null && f.Bytes.HasValue && (f.IsSample == null || !(bool)f.IsSample)
                                           select f).Count(),
                        AssetFilesAllTimeCount = (from ft in db.FileUploadLogs where ft.AccountId == accountThing.Id && ft.FileThingType == Things.ThingType(typeof(FileThing)) select ft).Count()
                    }).Fill(data =>
                    {
                        var accountThing = things[data.AccountId].CastTo<AccountThing>();
                        accountThing.VideoCount = data.VideoCount;
                        accountThing.VideoCountBillable = data.VideoCountBillable;
                        accountThing.VideosAddedInBillingCycle = data.VideosAddedInBillingCycle;
                        accountThing.ProductionCount = data.ProductionCount;
                        accountThing.ProductionCountBillable = data.ProductionCountBillable;
                        accountThing.AssetBytesTotal = data.AssetBytesTotal;
                        accountThing.AssetsTotalSizeBillable = new FileSize(data.AssetBytesTotalBillable);
                        accountThing.AssetFilesCount = data.AssetFilesCount;
                        accountThing.AssetFilesAllTimeCount = data.AssetFilesAllTimeCount;

                    }).AddTo(fillers);
        }

        private static void FillThingData(int id, object data, Dictionary<int, ThingBase> things)
        {
            ThingBase thing;
            if (things.TryGetValue(id, out thing))
            {
                thing.FillPropertiesWithRecord(data);
            }
        }

        
        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.Account()), InsertIdParam);
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.Accounts where d.ThingId == Id select d).Single();
            FillRecordWithProperties(data);
        }


        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.Account
            {
                ThingId = Id
            });
        }

        protected override void OnChildAdded(ThingBase child)
        {
            base.OnChildAdded(child);
            child.IfType<FeatureAccessThing>(f => 
                {
                    if (f.Name.HasChars())
                        FeaturesWithAccess.Add(f.Name);
                });
        }

        protected override void OnChildRemoved(ThingBase child)
        {
            base.OnChildRemoved(child);
            child.IfType<FeatureAccessThing>(f =>
            {
                if (f.Name.HasChars())
                    FeaturesWithAccess.Remove(f.Name);
            });
        }


        /// <summary>
        /// Changes their plan to the new one.  This is useful for upgrades and downgrades.  Doesn't check whether the new plan allows upgrades so this can be used by our backend.  Also doesn't check downgrade constraints.
        /// </summary>
        /// <param name="planId"></param>
        public void ChangePlan(string planId, bool callUpdate)
        {
            TrackChanges();

            var currentPlan = Plan;
            var newPlan = IoC.Get<IPlansRepository>().Get(planId);
            if (newPlan == null)
                throw new ArgumentException("Plan could not be found.");
            if (planId.EqualsCaseSensitive(PlanId))
                throw new ArgumentException("The account is already on this plan.");

            //for free plans we cancel their current subscription and don't sign them back up
            if (newPlan.PriceInCents <= 0)
            {
                if (this.IsSubscribed())
                {
                    RecurlySubscription.CancelSubscription(Id.ToInvariant());
                    Status = AccountStatus.FreePlan;                    
                }
            }
            else if (this.IsSubscribed())
            {
                var isUpgrade = newPlan.PriceInCents > Plan.PriceInCents;
                //for upgrades we cancel the subscription and start it fresh.  we liked this better than the options recurly had
                var subscription = new RecurlySubscription(new RecurlyAccount(Id.ToInvariant()))
                {
                    PlanCode = planId,
                    Quantity = 1,
                };
                if (isUpgrade)
                {
                    //todo: log it
                    subscription.ChangeSubscription(RecurlySubscription.ChangeTimeframe.Now);
                    this.CurrentBillingPeriodStartedOn = DateTime.UtcNow;//billing cycle resets to start today
                }
                else//for downgrades we don't prorate and just change the subscription with a timing of end of month
                {
                    subscription.ChangeSubscription(RecurlySubscription.ChangeTimeframe.Renewal);
                }
            }

            //todo: log it
            this.PlanId = planId;
            if ( callUpdate )
                this.Update();

        }


        /// <summary>
        /// Indicates whether this account meets the limits of the given plan.  Returns null if it's good.  Otherwise it returns a list of criteria that it exceeds.
        /// String results can contain: "Productions","Videos","Assets"
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public string[] CanDowngradeTo(IPlan plan)
        {
            List<string> exceeds = new List<string>();
            if (plan.ProductionLimit.HasValue && plan.ProductionLimit.Value < ProductionCountBillable)
            {
                exceeds.Add("Productions");
            }
            if (plan.VideosTotalMaxCount.HasValue && plan.VideosTotalMaxCount.Value < VideoCountBillable)
            {
                exceeds.Add("Videos");
            }
            if (plan.AssetsTotalMaxSize.HasValue && plan.AssetsTotalMaxSize.Value < AssetsTotalSizeBillable)
            {
                exceeds.Add("Assets");
            }
            return exceeds.Count == 0 ? null : exceeds.ToArray();
        }


        IImage IAccount.Logo
        {
            get { return Logo; }
        }


    }


    public class AccountSuspensionReasons
    {
        public const string TRIAL_EXPIRED = "TrialExpired";

    }



}