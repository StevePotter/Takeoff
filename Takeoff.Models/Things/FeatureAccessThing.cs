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
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Takeoff.Models
{

    /// <summary>
    /// Used to give an account access to a beta feature.
    /// </summary>
    [ThingType("FeatureAccess")]
    [Serializable]
    public class FeatureAccessThing : ThingBase
    {
        #region Constructors

        public FeatureAccessThing()
        {
        }

        protected FeatureAccessThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
        
        /// <summary>
        /// The name of the feature that is activated.
        /// </summary>
        public string Name
        {
            get
            {
                return NameProperty.GetValue(this);
            }
            set
            {
                NameProperty.SetValue(this, value);
            }
        }
        private string _Name;
        private static readonly ThingDataProperty<string, FeatureAccessThing, Data.FeatureAccess> NameProperty = new ThingDataProperty<string, FeatureAccessThing, Data.FeatureAccess>()
        {
            Name = "Name",
            SetField = (o, v) => o._Name = v,
            GetField = o => o._Name,
            GetData = o => o.Name,
            SetData = (o, v) => o.Name = v,
        }.Register();

        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.FeatureAccess>(db);
        }

        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.FeatureAccess()), InsertIdParam);
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            FillRecordWithProperties((from d in db.FeatureAccesses where d.ThingId == Id select d).Single());
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.FeatureAccess
            {
                ThingId = Id
            });
        }

  
    }


}