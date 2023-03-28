using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Takeoff.Models
{

    /// <summary>
    /// Provides a higher level of property management than typical CLR properties.  Has the ability to track whether the property has been set, "updated", and automates certain repetitive tasks like serialization and interfacing with data records.
    /// When combined with FancyType, this reduces lots of repetitive code.
    /// </summary>
    /// <remarks>
    /// When you create a property, you must call Register on it.
    /// </remarks>
    [DebuggerDisplay("{Name}  {PropertyType.Name}")]
    public abstract partial class ThingProperty
    {
        protected ThingProperty()
        {
        }

        #region Fields

        string m_name;
        
        ThingPropertyContainerMetaData m_DeclaringTypeData;
        int? m_typeHeirarchyIndex;
        Type m_DeclaringType;
        Type m_PropertyType;
        ThingPropertyContainerMetaData m_propertyTypeData;
        bool m_propertyTypeDataValid;
        TypeCode? m_typeCode;


        int? m_indexForIsSet;
        int? m_bitForIsSet;
        int? m_indexForValueInObjectArray;
        bool m_sealed;

        #endregion

        #region Properties


        /// <summary>
        /// The name of the property.  This should be the same name as its cooresponding CLR property.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();

                m_name = value;
            }
        }


        /// <summary>
        /// If true, this is a basic set-get property.  
        /// </summary>
        /// <remarks>Note that in certain cases you could have a complex type that is a simple property.  This is typically for object passing.  For example, if you have a Grid object all over the place that is just there for a reference to the grid.   In that case, you don't want to treat it as complex, or else the state saving would turn into an infinite loop.</remarks>
        public bool IsSimple
        {
            get
            {
                return m_isSimple.GetValueOrDefault(true);
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();

                m_isSimple = value;
            }
        }
        bool? m_isSimple;

        /// <summary>
        /// The index of this property within its OwnerTypeData.Properties collection.
        /// </summary>
        /// <remarks>Set when this property is registered.</remarks>
        public int Index { get; private set; }

        /// <summary>
        /// The index of this property within all of the properties for this type as well as super types.
        /// </summary>
        public int TypeHeirarchyIndex
        {
            get
            {
                var index = m_typeHeirarchyIndex;
                if (!index.HasValue)
                {
                    ThingProperty previousProperty = GetPreviousProperty(true);
                    index = m_typeHeirarchyIndex = previousProperty == null ? 0 : previousProperty.TypeHeirarchyIndex + 1;
                }
                return index.Value;
            }
        }

        /// <summary>
        /// The ComponentType meta data for the DeclaringType.  Keep in mind that if a class inherits from this type, the property's DeclaringType will be the base class, so querying it for properties will not include all the props in the sub class.
        /// </summary>
        public ThingPropertyContainerMetaData DeclaringTypeData
        {
            get
            {
                return m_DeclaringTypeData;
            }
            private set
            {
                Debug.Assert(!Sealed, "Cannot set after it is sealed.");
                m_DeclaringTypeData = value;
            }
        }

        /// <summary>
        /// The object type that declares this property.  Keep in mind that if a class inherits from this type, the property's DeclaringType will be the base class, so querying it for properties will not include all the props in the sub class.
        /// </summary>
        public Type DeclaringType
        {
            get
            {
                return m_DeclaringType;
            }
            protected set
            {
                Debug.Assert(!Sealed, "Cannot set after it is sealed.");
                m_DeclaringType = value;
            }
        }

        /// <summary>
        /// The property value's type.  This is the same as its cooresponding CLR property's type.
        /// </summary>
        public Type PropertyType
        {
            get
            {
                return m_PropertyType;
            }
            protected set
            {
                Debug.Assert(!Sealed, "Cannot set after it is sealed.");
                m_PropertyType = value;
            }
        }

        /// <summary>
        /// Indicates whether the property is a read-only property.  
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return !CanWrite;
            }
        }

        /// <summary>
        /// Indicates whether this property can be written to.
        /// </summary>
        public bool CanWrite
        {
            get
            {
                return m_CanWrite.GetValueOrDefault();
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();

                m_CanWrite = value;
            }
        }
        bool? m_CanWrite;

        /// <summary>
        /// The FancyType metadata for the PropertyType.  For outside/primitive types like int or string, this will be null.
        /// </summary>
        protected internal ThingPropertyContainerMetaData PropertyTypeData
        {
            get
            {
                if (m_propertyTypeDataValid)
                    return m_propertyTypeData;

                var data = m_propertyTypeData = ThingPropertyContainerMetaData.GetMetaData(PropertyType);
                m_propertyTypeDataValid = true;
                return data;
            }
        }

        /// <summary>
        /// The TypeCode for the PropertyType value;
        /// </summary>
        protected TypeCode PropertyTypeCode
        {
            get
            {
                if (m_typeCode.HasValue)
                    return m_typeCode.Value;

                m_typeCode = Type.GetTypeCode(PropertyType);
                return m_typeCode.Value;
            }
        }

        /// <summary>
        /// Indicates whether this property will track whether or not it has been set.  The set bits are stored in the owning object in a bit vector.
        /// </summary>
        public bool TrackIfIsSet
        {
            get
            {
                return _TrackIfIsSet.GetValueOrDefault();
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();

                _TrackIfIsSet = value;
            }
        }
        protected bool? _TrackIfIsSet;

        /// <summary>
        /// Gets the int bit value for IndexForIsSet, which can be fed directly to a bit vector and guarantee a unique flag.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]//hides from the debugger because if you view the ThingProperty in a watch, it could cause this property to resolve incorrectly.
        internal int BitForIsSet
        {
            get
            {
                Debug.Assert(Sealed, "Do not access this until the property is sealed or else it may be incorrect.");
                Debug.Assert(TrackIfIsSet);

                var bit = m_bitForIsSet;
                if (bit.HasValue)
                    return bit.Value;

                bit = m_bitForIsSet = (int)Math.Pow(2.0, (double)IndexForIsSet);
                return bit.Value;
            }
        }

        /// <summary>
        /// Gets the ordinal of this property for all properties that track whether they have been set or not.
        /// </summary>
        /// <returns></returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]//hides from the debugger because if you view the ThingProperty in a watch, it could cause this property to resolve incorrectly.
        internal int IndexForIsSet
        {
            get
            {
                Debug.Assert(Sealed, "Do not access this until the property is sealed or else it may be incorrect.");
                Debug.Assert(TrackIfIsSet);

                var index = m_indexForIsSet;
                if (index.HasValue)
                    return index.Value;

                Predicate<ThingProperty> filter = (prop) =>
                {
                    return prop.TrackIfIsSet;
                };
                var previousProp = GetPreviousProperty(filter, true);
                index = m_indexForIsSet = previousProp == null ? 0 : previousProp.IndexForIsSet + 1;

                return index.Value;
            }
        }



        /// <summary>
        /// Indicates whether the object does not allow modifications.
        /// </summary>
        protected bool Sealed
        {
            get
            {
                return m_sealed;
            }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Performs the operation to seal this property from future modifications.
        /// </summary>
        private void PerformSeal()
        {
            OnSealing();
            Seal();
        }

        /// <summary>
        /// "Seals" (not to be confused with C# seal) to ensure the property doesn't change after this point, essentially making it immutable.
        /// </summary>
        /// <remarks>
        /// Altering properties after sealing would potentially screw up system behavior, so public properties check for sealing and throw an exception it's the property is set after sealing.
        /// Right before sealing, certain properties should be initialized.
        /// </remarks>
        protected virtual void Seal()
        {
            Debug.Assert(!m_sealed);
            m_sealed = true;           
        }

        protected virtual void OnSealing()
        {
            if (!m_isSimple.HasValue)
            {
                IsSimple = PropertyTypeData == null;
            }

            if (!m_CanWrite.HasValue)
            {
                CanWrite = DetermineCanWrite();
            }

        }


        /// <summary>
        /// Gets a value indicating whether this property should be able to be written to, which is determined by some combination of settings.
        /// </summary>
        /// <returns></returns>
        protected abstract bool DetermineCanWrite();

        /// <summary>
        /// Gets the previous property in the property heiarchy.  
        /// </summary>
        protected ThingProperty GetPreviousProperty(bool searchBaseClasses)
        {
            return GetPreviousProperty(null, searchBaseClasses);
        }

        /// <summary>
        /// Gets the previous property of the given type in the property/type heirarchy that matches the filter provided.
        /// </summary>
        protected ThingProperty GetPreviousProperty(Predicate<ThingProperty> match, bool searchBaseClasses)
        {
            ThingPropertyContainerMetaData ownerData = DeclaringTypeData;
            int propertyIndex = Index;
            if (propertyIndex > 0)
            {
                for (int i = propertyIndex - 1; i >= 0; i--)
                {
                    ThingProperty property = ownerData.PropertiesAtClassLevel[i];
                    if (match == null || match(property))
                        return property;
                }
            }

            if (!searchBaseClasses)
                return null;

            ThingPropertyContainerMetaData baseTypeData = ownerData.BaseTypeMetaData;
            while (baseTypeData != null)
            {
                if (baseTypeData.HasProperties)
                {
                    var properties = baseTypeData.PropertiesAtClassLevel;
                    for (int i = properties.Length - 1; i >= 0; i--)
                    {
                        ThingProperty property = baseTypeData.PropertiesAtClassLevel[i];
                        if (match == null || match(property))
                            return property;
                    }
                }
                baseTypeData = baseTypeData.BaseTypeMetaData;
            }
            return null;

        }

        /// <summary>
        /// Gets the value from the given declaring object.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        internal abstract object GetValueAsObject(object owner);

        /// <summary>
        /// Sets the value on the declaring object.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="value"></param>
        internal abstract void SetValueAsObject(object owner, object value);

        /// <summary>
        /// Resets the value in the declaring object, clearing whatever fields is necessary.
        /// </summary>
        /// <param name="owner"></param>
        public abstract void ResetValue(object owner);

        public bool IsSet(IThingPropertyContainer owner)
        {
            //todo: throw exception if not tracking this
            return owner.SetPropertyBits[BitForIsSet];
        }


        #endregion

        #region Serialization

        /// <summary>
        /// Gets the serialized value for this property from the given jObject 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="json"></param>
        public abstract void DeserializeJson(IThingPropertyContainer owner, JObject json);

        public abstract void DeserializeJsonServiceStack(IThingPropertyContainer owner, IDictionary<string, string> json);

        public abstract void SerializeJson(IThingPropertyContainer owner, JObject json);

        public abstract void SerializeJson2(IThingPropertyContainer owner, JsonWriter writer);
        
        public abstract void Serialize(IThingPropertyContainer owner, System.Runtime.Serialization.SerializationInfo info);

        public abstract void Deserialize(IThingPropertyContainer owner, object value, System.Runtime.Serialization.SerializationInfo info);

        #endregion

        #region Registration

        public static ThingProperty<TProperty, TDeclarer> RegisterBasicProperty<TProperty, TDeclarer>(string name) where TDeclarer : IThingPropertyContainer
        {
            var prop = new ThingProperty<TProperty, TDeclarer>
            {
                Name = name,
                IsSimple = true,
                CanWrite = true
            };
            Register(prop);
            return prop;
        }


        /// <summary>
        /// Registers the property.  Doesn't set anything, so all properties must be set beforehand.
        /// </summary>
        /// <param name="property"></param>
        public static ThingProperty Register(ThingProperty property)
        {
            if (property.Sealed)
                throw new ArgumentException("Property was already registered.");

            var typeData = ThingPropertyContainerMetaData.GetMetaData(property.DeclaringType);
            property.DeclaringTypeData = typeData;
            property.Index = typeData.AddProperty(property);

            property.PerformSeal();

            return property;
        }

        #endregion

    }


    public class CannotModifySealedObjectException : InvalidOperationException
    {

    }


}
