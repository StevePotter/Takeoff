using System;
using System.Collections.Generic;

using System.Text;
using System.Reflection;

using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;


namespace Takeoff.Models
{

    /// <summary>
    /// Property with basic set/get value.  Values can be primitive types, arrays, etc. 
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <typeparam name="TProperty"></typeparam>
    /// <typeparam name="TDeclarer"></typeparam>
    public partial class ThingProperty<TProperty, TDeclarer>: ThingProperty where TDeclarer : IThingPropertyContainer
    {
        public ThingProperty()
        {
            DeclaringType = typeof(TDeclarer);
            PropertyType = typeof(TProperty);
        }


        #region Fields

        Func<TDeclarer, TProperty> m_getValueCallback;
        Action<TDeclarer, TProperty> m_setValueCallback;

        #endregion

        #region Properties

        /// <summary>
        /// The delegate that returns the property backing value for an instance of the owner.  
        /// </summary>
        /// <remarks>
        /// For basic get/set properties, this should return the backing field because returning the property would result in an infinite loop.
        /// For complex read-only properties, this can return the property value if the CLR property's get method doesn't use the GetValue of the property.
        /// </remarks>
        public Func<TDeclarer, TProperty> GetField
        {
            get
            {
                return m_getValueCallback;
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();

                m_getValueCallback = value;
            }
        }

        /// <summary>
        /// The delegate that sets the property backing value for an instance of the owner.  
        /// </summary>
        /// <remarks>
        /// For simple get/set properties, this should set the backing field because setting the property would result in an infinite loop.
        /// For read only properties, this doesn't need to be set.  
        /// 
        /// For get/set properties, both the Get/Set callbacks should be set, not one or the other.
        /// </remarks>
        public Action<TDeclarer, TProperty> SetField
        {
            get
            {
                return m_setValueCallback;
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();

                m_setValueCallback = value;

                CanWrite = value != null;
            }
        }


        /// <summary>
        /// Returns the default value from the owner if the value hasn't been set via SetValue.  This is useful when the default value is not just a constant (like Skin.Background).
        /// </summary>
        public Func<TDeclarer, TProperty> DefaultValueCallback
        {
            get
            {
                return m_DefaultValueCallback;
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();

                m_DefaultValueCallback = value;
            }
        }
        Func<TDeclarer, TProperty> m_DefaultValueCallback;

        /// <summary>
        /// If the default value is a constant, this is it.
        /// </summary>
        public TProperty DefaultValue
        {
            get
            {
                return m_DefaultValue;
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();
                m_DefaultValue = value;
                _IsDefaultValueSet = true;
            }
        }
        TProperty m_DefaultValue;
        bool _IsDefaultValueSet = false;



        /// <summary>
        /// Converts the current property value to something that can be serialized in JSON.
        /// </summary>
        public Func<TProperty, object> ToJson
        {
            get;
            set;
        }

        /// <summary>
        /// Converts the current property value to something that can be serialized in JSON.
        /// </summary>
        public Func<TProperty, object> ToJsonSimple2
        {
            get;
            set;
        }

        public Action<TProperty, JsonWriter> ToJsonComplex2
        {
            get;
            set;
        }

        /// <summary>
        /// Converts the value from a json-serialized value, reading from the source reader.  Sets properties as necessary.
        /// </summary>
        public Action<TDeclarer, JsonReader> FromJsonReader
        {
            get;
            set;
        }


        /// <summary>
        /// A way to custom serialize this property.  Won't be called if the property hasn't been set.
        /// </summary>
        public Func<TProperty, object> CustomSerialize
        {
            get;
            set;
        }


        /// <summary>
        /// Converts the value from a json-serialized value.  
        /// </summary>
        public Func<object, TProperty> FromJson
        {
            get;
            set;
        }

        /// <summary>
        /// Converts the value from a json-serialized value.  
        /// </summary>
        public Func<string, TProperty> FromJsonServiceStack
        {
            get;
            set;
        }



        public IEqualityComparer<TProperty> EqualityComparer
        {
            get
            {
                return _EqualityComparer;
            }
            set
            {
                if (!CheckForEquality)
                    throw new InvalidOperationException("Cannot set comparer when CheckForEquality is false.");
                _EqualityComparer = value;
            }
        }
        public IEqualityComparer<TProperty> _EqualityComparer;


        /// <summary>
        /// When true, we will try and check for equality to avoid marking properties as changed when they really weren't.
        /// </summary>
        public bool CheckForEquality
        {
            get
            {
                return _checkForEquality.GetValueOrDefault();
            }
            set
            {
                _checkForEquality = value;
            }
        }
        private bool? _checkForEquality;


        #endregion

        #region Events

        /// <summary>
        /// Occurs at the beginning of SetValue. 
        /// </summary>
        public event EventHandler<PropertySetEventArgs<TProperty, TDeclarer>> BeforeSet;

        /// <summary>
        /// Occurs at the end of SetValue. 
        /// </summary>
        public event EventHandler<PropertySetEventArgs<TProperty, TDeclarer>> AfterSet;


        #endregion

        #region Methods

        /// <summary>
        /// Gets the value of the property for the given owner.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public TProperty GetValue(TDeclarer owner)
        {
            if (!TrackIfIsSet || owner.SetPropertyBits[BitForIsSet])
            {
                return GetField(owner);
            }

            var callback = DefaultValueCallback;
            return callback == null ? DefaultValue : callback(owner);

        }

        /// <summary>
        /// Sets the property value on the given over.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="value"></param>
        public void SetValue(TDeclarer owner, TProperty value)
        {
            if (!CanWrite)
                throw new InvalidOperationException("Property was read only.");

            if (BeforeSet != null)
                BeforeSet(this, new PropertySetEventArgs<TProperty, TDeclarer>() { Owner = owner, NewValue = value });

            //lots of times you will blindly set a property that hasn't changed.  if the value hasn't changed then we don't mark it that way.  
            bool markAsChanged = false, trackChanges = TrackChanges && owner.IsTrackingPropertyChanges;
            if ( trackChanges )
            {
                if ( EqualityComparer != null && IsSet(owner) )
                {
                    var currValue = GetValue(owner);
                    markAsChanged = !EqualityComparer.Equals(currValue, value);
                }
            }

            SetField(owner, value);

            if (TrackIfIsSet)
            {
                var bits = owner.SetPropertyBits;
                bits[BitForIsSet] = true;
                owner.SetPropertyBits = bits;
            }

            if (markAsChanged)
            {
                var bits = owner.PropertiesChangedSinceTracking;
                bits[BitForIsSet] = true;
                owner.PropertiesChangedSinceTracking = bits;
            }

            if (AfterSet != null)
                AfterSet(this, new PropertySetEventArgs<TProperty, TDeclarer>() { Owner = owner, NewValue = value });
        }


        /// <summary>
        /// Takes the value, which may or may not be of the current type, and converts it to the proper type.
        /// </summary>
        public TProperty ConvertValue(object value)
        {
            if (value == null)
            {
                return default(TProperty);
            }
            var valueType = value.GetType();
            if (valueType.Equals(PropertyType))
            {
                return (TProperty)value;
            }

            //if the property type is a nullable value, then create the new nullable type
            var nullableType = Nullable.GetUnderlyingType(PropertyType);
            if (nullableType != null)
            {
                NullableConverter nullableConverter = new NullableConverter(PropertyType);
                return (TProperty)Convert.ChangeType(value, nullableConverter.UnderlyingType);
            }
            else
            {
                return (TProperty)Convert.ChangeType(value, PropertyType);
            }
        }


        internal override object GetValueAsObject(object owner)
        {
            return (object)GetValue((TDeclarer)owner);
        }

        internal override void SetValueAsObject(object owner, object value)
        {
            SetValue((TDeclarer)owner, ConvertValue(value));
        }

        protected override void OnSealing()
        {
            base.OnSealing();

            if (GetField == null && IsSimple)
                throw new InvalidOperationException("GetValueCallback is required for this property.");

            if (!_TrackIfIsSet.HasValue)
            {
                TrackIfIsSet = CanWrite || (SetField != null && (DefaultValueCallback != null || _IsDefaultValueSet));//if it's simple and has a GetValueCallback and also a default value was specified, we need to track isset
            }

            var propertyType = typeof(TProperty);
            var typecode = Type.GetTypeCode(propertyType);
            //simple types like string, value types, and nullables get equality checked.  others need to do it themselves
            if ( !_checkForEquality.HasValue )
                CheckForEquality = typecode != TypeCode.Object && typecode != TypeCode.Empty && Nullable.GetUnderlyingType(propertyType) == null;
            if ( CheckForEquality && _EqualityComparer == null )
                _EqualityComparer = System.Collections.Generic.EqualityComparer<TProperty>.Default;
        }

        protected override bool DetermineCanWrite()
        {
            return SetField != null || (IsSimple && GetField == null); //assume that simple properties can be set and complex ones can't.  note that in the generic class, if you set the SetValueCallback, it sets CanWrite
        }


        public override void ResetValue(object owner)
        {
            Debug.Assert(owner is TDeclarer);
            TDeclarer castedOwner = (TDeclarer)owner;

            if (IsSimple)
            {
                //note that the property field is still set...but getvalue will go to default
                var bits = castedOwner.SetPropertyBits;
                bits[BitForIsSet] = false;
                castedOwner.SetPropertyBits = bits;
            }
            else
            {
                TProperty propertyValue = GetValue(castedOwner);

                //todo: put this into ComponentType so pre/post processors can be added
                foreach (ThingProperty currProperty in PropertyTypeData.AllProperties())
                {
                    currProperty.ResetValue(propertyValue);
                }
            }
        }


        /// <summary>
        /// Registers the property with the system.  This can only be called once and the property's settings should 
        /// not change afterward. 
        /// </summary>
        /// <remarks>
        /// This was created so you can use a type initializer in the code that declares the property, like:
        /// 
        ///static readonly ThingProperty<Color, StyleSettings> BackgroundColorProperty = new ThingProperty<Color, StyleSettings>()
        ///{
        ///    Name = "BackgroundColor",
        ///    IsSimple = true,
        ///    CanWrite = true,
        ///}.Register();
        /// 
        /// </remarks>
        public ThingProperty<TProperty, TDeclarer> Register()
        {
            ThingProperty.Register(this);
            return this;
        }

        public override void DeserializeJson(IThingPropertyContainer owner, JObject json)
        {
            var jsonName = this.Name;
            var jsonObject = json[jsonName];
            if ( jsonObject == null )
                return;
            var jsonValue = jsonObject as JValue;
            var castedOwner = (TDeclarer)owner;
            TProperty value;
            if (FromJson != null)
            {
                value = FromJson(jsonValue == null ? jsonObject : jsonValue.Value);
            }
            else
            {
                //all json dates are serialized in utc but i guess due to a bug or something, they aren't read back with Kind of UTC.  this is a crappy hack but works fine
                if (jsonValue != null && jsonValue.Value != null && jsonValue.Value is DateTime)
                {
                    var date = (DateTime)jsonValue.Value;
                    jsonValue.Value = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                }

                value = ConvertValue(jsonValue == null ? jsonObject : jsonValue.Value);
            }
            SetValue(castedOwner, value);
        }


        public override void DeserializeJsonServiceStack(IThingPropertyContainer owner, IDictionary<string, string> json)
        {
            var jsonName = this.Name;
            string jsonValue;
            if (!json.TryGetValue(jsonName, out jsonValue))
                return;
            var castedOwner = (TDeclarer)owner;
            TProperty value;
            if (FromJsonServiceStack != null)
            {
                value = FromJsonServiceStack(jsonValue);
            }
            else
            {
                value = ServiceStack.Text.JsonSerializer.DeserializeFromString<TProperty>(jsonValue);
                //todo:
                //all json dates are serialized in utc but i guess due to a bug or something, they aren't read back with Kind of UTC.  this is a crappy hack but works fine
                if (value is DateTime)
                {
                    var date = (DateTime)(object)value;
                    if (date.Kind != DateTimeKind.Utc)
                    {
                        value = (TProperty)(object)DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    }
                }
            }
            SetValue(castedOwner, value);
        }


        public override void SerializeJson(IThingPropertyContainer owner, JObject json)
        {
            if (!IsSet(owner))
                return;

            if (ToJson != null)
            {
                //if any of the values are jobject native objects, then use them directly.  otherwise 
                var jsonValue = ToJson(GetValue((TDeclarer)owner));
                var asToken = jsonValue as JToken;
                if (asToken == null)
                    json[this.Name] = new JValue(jsonValue);
                else
                    json[this.Name] = asToken;
            }
            else
            {
                json[this.Name] = new JValue(this.GetValueAsObject(owner));
            }
        }

        public override void SerializeJson2(IThingPropertyContainer owner, JsonWriter writer)
        {
            if (!IsSet(owner))
                return;

            writer.WritePropertyName(this.Name);

            if (ToJsonSimple2 != null)
            {
                writer.WriteValue(ToJson(GetValue((TDeclarer)owner)));
            }
            else if ( ToJsonComplex2 != null )
            {
                ToJsonComplex2(GetValue((TDeclarer) owner), writer);
            }
            else
            {
                writer.WriteValue(this.GetValueAsObject(owner));
            }            
        }


        

        public override void Serialize(IThingPropertyContainer owner, System.Runtime.Serialization.SerializationInfo info)
        {
            if (!IsSet(owner))
                return;

            if (CustomSerialize != null)
            {
                info.AddValue(Name, CustomSerialize(GetValue((TDeclarer)owner)));
            }
            else
            {
                info.AddValue(Name, this.GetValueAsObject(owner));
            }
        }

        public override void Deserialize(IThingPropertyContainer owner, object value, System.Runtime.Serialization.SerializationInfo info)
        {
            this.SetValue(owner.CastTo<TDeclarer>(), ConvertValue(value));
            //var jsonName = this.Name;
            //if ( info.
            //info.GetValue(this.Name, PropertyType);

            //var jsonObject = json[jsonName];
            //if (jsonObject == null)
            //    return;
            //var jsonValue = jsonObject as JValue;
            //var castedOwner = (TDeclarer)owner;
            //TProperty value;
            //if (FromJson != null)
            //{
            //    value = FromJson(jsonValue == null ? jsonObject : jsonValue.Value);
            //}
            //else
            //{
            //    //all json dates are serialized in utc but i guess due to a bug or something, they aren't read back with Kind of UTC.  this is a crappy hack but works fine
            //    if (jsonValue != null && jsonValue.Value != null && jsonValue.Value is DateTime)
            //    {
            //        var date = (DateTime)jsonValue.Value;
            //        jsonValue.Value = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            //    }

            //    value = ConvertValue(jsonValue == null ? jsonObject : jsonValue.Value);
            //}
            //SetValue(castedOwner, value);
        }

        #endregion



    }

    public class PropertySetEventArgs<TProperty, TOwner> : EventArgs
    {
        public TProperty NewValue { get; set; }
        public TOwner Owner { get; set; }
    }

}
