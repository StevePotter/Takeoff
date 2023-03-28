using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Takeoff.Models
{
    /// <summary>
    /// Shown to users one time when they next log in.  Then it gets deleted.  This is useful for asking for a survey, information, etc.
    /// </summary>
    [ThingType("ViewPrompt")]
    [Serializable]
    public class ViewPromptThing : ThingBase, IViewPrompt
    {
        #region Constructors

        public ViewPromptThing()
        {
        }

        protected ViewPromptThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        public string View
        {
            get
            {
                return ViewProperty.GetValue(this);
            }
            set
            {
                ViewProperty.SetValue(this, value);
            }
        }
        private string _View;
        private static readonly ThingDataProperty<string, ViewPromptThing, Data.ViewPrompt> ViewProperty = new ThingDataProperty<string, ViewPromptThing, Data.ViewPrompt>()
        {
            Name = "View",
            SetField = (o, v) => o._View = v,
            GetField = o => o._View,
            GetData = o => o.View,
            SetData = (o, v) => o.View = v,
        }.Register();


        public DateTime? ExpiresOn
        {
            get
            {
                return ExpiresOnProperty.GetValue(this);
            }
            set
            {
                ExpiresOnProperty.SetValue(this, value);
            }
        }
        private DateTime? _ExpiresOn;
        private static readonly ThingDataProperty<DateTime?, ViewPromptThing, Data.ViewPrompt> ExpiresOnProperty = new ThingDataProperty<DateTime?, ViewPromptThing, Data.ViewPrompt>()
        {
            Name = "ExpiresOn",
            SetField = (o, v) => o._ExpiresOn = v,
            GetField = o => o._ExpiresOn,
            ShouldGetData = r => r.ExpiresOn != null,
            GetData = o => o.ExpiresOn,
            SetData = (o, v) => o.ExpiresOn = v,
        }.Register();


        public DateTime? StartsOn
        {
            get
            {
                return StartsOnProperty.GetValue(this);
            }
            set
            {
                StartsOnProperty.SetValue(this, value);
            }
        }
        private DateTime? _StartsOn;
        private static readonly ThingDataProperty<DateTime?, ViewPromptThing, Data.ViewPrompt> StartsOnProperty = new ThingDataProperty<DateTime?, ViewPromptThing, Data.ViewPrompt>()
        {
            Name = "StartsOn",
            SetField = (o, v) => o._StartsOn = v,
            GetField = o => o._StartsOn,
            ShouldGetData = r => r.StartsOn != null,
            GetData = o => o.StartsOn,
            SetData = (o, v) => o.StartsOn = v,
        }.Register();

        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return this.CreateAuxDataFiller<Data.ViewPrompt>(db);
        }

        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.ViewPrompt()), InsertIdParam);
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.ViewPrompts where d.ThingId == Id select d).Single();
            FillRecordWithProperties(data);
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.ViewPrompt
            {
                ThingId = Id
            });
        }


    }
}