using System.Diagnostics;
namespace Takeoff.Models
{
    partial class DataModelBase
    {
#if DEBUG
        partial void OnCreated()
        {
            this.Log = new DebuggerWriter();
        }
#endif

    }


    public class ActionSourceFromData
    {
        /// <summary>
        /// The ID of the change source, NOT the "bubbler" change record.
        /// </summary>
        public int Id { get; set; }

        public System.DateTime Date { get; set; }

        public int UserId { get; set; }

        public int ThingId { get; set; }

        public int ProductionId { get; set; }

        public string ThingType { get; set; }

        public System.Nullable<int> ThingParentId { get; set; }

        public string Action { get; set; }

        public string Data { get; set; }

        public string Description { get; set; }
    }


    /// <summary>
    /// Fields that are computed through complex data queries.  Values are then saved in memcached.
    /// </summary>
    public class AccountComputedData
    {
        public int AccountId { get; set; }
        public int VideoCount { get; set; }
        public int VideoCountBillable { get; set; }
        public int VideosAddedInBillingCycle { get; set; }
        public int ProductionCount { get; set; }
        public int ProductionCountBillable { get; set; }
        public long AssetBytesTotal { get; set; }
        public long AssetBytesTotalBillable { get; set; }
        public int AssetFilesCount { get; set; }
        public int AssetFilesAllTimeCount { get; set; }

    }
}
