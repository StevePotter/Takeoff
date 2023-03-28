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

    [ThingType("Setting")]
    [Serializable]
    public class SettingThing : ThingBase, ISetting
    {

                #region Constructors

        public SettingThing()
        {
        }

        protected SettingThing(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {  
        }

        #endregion
       

        /// <summary>
        /// The name of the setting.
        /// </summary>
        public string Key
        {
            get
            {
                return KeyProperty.GetValue(this);
            }
            set
            {
                KeyProperty.SetValue(this, value);
            }
        }
        private string _Key;
        private static readonly ThingDataProperty<string, SettingThing, Data.Setting> KeyProperty = new ThingDataProperty<string, SettingThing, Data.Setting>()
        {
            Name = "Key",
            SetField = (o, v) => o._Key = v,
            GetField = o => o._Key,
            GetData = o => o.Key,
            SetData = (o, v) => o.Key = v,
        }.Register();
	
        /// <summary>
        /// Value of the setting.
        /// </summary>
        //Note: this is not a data property because the logic to set/get the data value is kinda funky
        public object Value
        {
            get
            {
                return ValueProperty.GetValue(this);
            }
            set
            {
                ValueProperty.SetValue(this, value);
            }
        }
        private object _RawValue;
        private object _ConvertedValue;
        private bool _DidConvertValue;

        private static readonly ThingProperty<object, SettingThing> ValueProperty = new ThingProperty<object, SettingThing>()
        {
            Name = "Value",
            SetField = (o, v) =>
                           {
                               o._DidConvertValue = false;
                               o._RawValue = v;
                           },
            GetField = o =>
                           {
                               if (!o._DidConvertValue)
                               {
                                   var def = o.Definition; //this was added for enums being deserialized from Json
                                   if (def == null)
                                       return o._RawValue;

                                   o._ConvertedValue = def.ConvertValue(o._RawValue);
                                   o._DidConvertValue = true;
                               }
                               return o._ConvertedValue;
                           }

        }.Register();


        public SettingDefinition Definition
        {
            get
            {
                if (_definition == null && Key.HasChars())
                {
                    SettingDefinitions.Definitions.TryGetValue(Key, out _definition);
                }
                return _definition;
            }
        }
        [NonSerialized]
        private SettingDefinition _definition;

        /// <summary>
        /// Takes the Value and converts it to a string for DB storage.
        /// </summary>
        /// <returns></returns>
        private string ConvertValue()
        {
            if ( Value == null )
                return null;

            if (Definition != null && Definition.Type != typeof(string))
            {
                //http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx
                var converter = TypeDescriptor.GetConverter(Definition.Type);
                return converter.ConvertToInvariantString(Value);
            }
            else
            {
                return Value.ToString();
            }
        }


        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;
            yield return new ThingAuxDataFiller<Setting>(db, Id, (d) =>
                                                                     {
                                                                         FillPropertiesWithRecord(d);
                                                                         SetValueFromData(d.Value);
                                                                     });
        }

        /// <summary>
        /// Only for filling values from data.
        /// </summary>
        /// <param name="data"></param>
        public void SetValueFromData(string data)
        {
            if (data.HasChars() && Definition != null && Definition.Type != typeof (string))
            {
                try
                {
                    var converter = TypeDescriptor.GetConverter(Definition.Type);
                    Value = converter.ConvertFromInvariantString(data);
                }
                catch
                {
                    //problem converting value...hide it to avoid screwing up the select for an entire thing tree
                }
            }
            else
            {
                Value = data;
            }
        }

        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(new Setting
            {
                Key = Key,
                Value = ConvertValue(),
                ApplicationId = ApplicationEx.AppName
            }, InsertIdParam);
        }

        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.Settings where d.ThingId == Id select d).Single();
            data.Key = Key;
            data.Value = ConvertValue();
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.Setting
            {
                ThingId = Id
            });
        }

        public static void UpdateSetting(string key, string formValue, UserThing user)
        {
            var definition = SettingDefinitions.Definitions[key];
            UpdateSetting(definition, formValue, user);
        }

        public static void UpdateSetting(SettingDefinition definition, string formValue, UserThing user)
        {
            var setting = user.GetSetting(definition.Name);//the actual settingthing, which could be null
            var newValue = definition.ConvertFromFormValue(formValue);
            if (setting != null && setting.Value.ValueEquals(newValue))
            {
                return;
            }

            if (newValue.ValueEquals(definition.Default))
            {
                //the setting is the same as the value, so delete the setting if it's there
                if (setting != null)
                    setting.Delete();
            }
            else
            {
                if (setting == null)
                {
                    setting = user.AddChild(new SettingThing
                    {
                        CreatedByUserId = user.Id,
                        CreatedOn = DateTime.UtcNow,
                        Key = definition.Name,
                        Value = newValue
                    }).Insert();
                }
                else
                {
                    setting.TrackChanges();
                    setting.Value = newValue;
                    setting.Update();
                }
            }
        }

    }

}
