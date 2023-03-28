using System;
using System.Collections.Generic;

using System.Text;
using System.Reflection;

using System.Xml;
using System.ComponentModel;
using System.Diagnostics;


namespace Takeoff.Models
{

    /// <summary>
    /// A thing property that has intimate knowledge about how it sets and gets it value from its underlying data model class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <typeparam name="TProperty"></typeparam>
    /// <typeparam name="TDeclarer"></typeparam>
    public partial class ThingDataProperty<TProperty, TDeclarer, TDataModel>: ThingProperty<TProperty, TDeclarer> where TDeclarer : IThingPropertyContainer
    {
        public ThingDataProperty():base()
        {
            DataModelType = typeof(TDataModel);
        }

        public Func<TDataModel, TProperty> GetData
        {
            get; set; 
        }

        /// <summary>
        /// A callback for setting the data value on a particular 
        /// </summary>
        public Action<TDataModel, TProperty> SetData
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates a callback that will test the data against default values.  If this returns false, the data value won't be set on the property.  This is used mostly for nullable values.
        /// </summary>
        public Func<TDataModel, bool> ShouldGetData
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the data record should be filled with this value.  If left alone, it will default to whether the property has been set.
        /// </summary>
        public Func<TDeclarer, bool> ShouldSetData
        {
            get;
            set;
        }


        public new ThingDataProperty<TProperty, TDeclarer, TDataModel> Register()
        {
            ThingProperty.Register(this);
            return this;
        }

        public override void FillWithRecord(IThingPropertyContainer owner, object dataRecord)
        {
            var model = (TDataModel)dataRecord;
            if (ShouldGetData != null && !ShouldGetData(model))
                return;
            var dataValue = GetData(model);
            SetValue((TDeclarer)owner, dataValue);
        }

        public override void FillRecord(IThingPropertyContainer owner, object dataRecord)
        {
            var castedOwner = (TDeclarer)owner;
            if ((ShouldSetData == null && IsSet(owner)) || (ShouldSetData != null && ShouldSetData(castedOwner)))
            {
                var record = (TDataModel)dataRecord;
                SetData(record, GetValue(castedOwner));
            }
        }
    }

}
