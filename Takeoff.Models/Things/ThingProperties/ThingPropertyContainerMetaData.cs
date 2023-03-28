using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Takeoff.Models
{
    [DebuggerDisplay("{Type.Name}, {Properties.Count}")]
    public partial class ThingPropertyContainerMetaData
    {

        public Type Type { get; internal set; }

        public ThingPropertyContainerMetaData BaseTypeMetaData { get; set; }

        List<ThingProperty> m_propertiesAtThisClassLevel;


        /// <summary>
        /// A list of properties, indexed by name, for this class as well as its base types.
        /// </summary>
        private Dictionary<string, ThingProperty> m_flattenedPropertiesByName;
        /// <summary>
        /// A list of properties, in order of their HeirarchyIndex, for this class as well as its base types.
        /// </summary>
        private ThingProperty[] m_propertiesFlattenedByIndex;

        /// <summary>
        /// Gets all the properties at the current class level.
        /// </summary>
        public ThingProperty[] PropertiesAtClassLevel
        {
            get
            {
                return m_propertiesAtThisClassLevel == null ? new ThingProperty[0] : m_propertiesAtThisClassLevel.ToArray();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>You shouldn't set values in this array.  This could have been a property but the debugger getting values would screw things up.</remarks>
        internal ThingProperty[] GetPropertiesFlattenedByIndex()
        {
            ThingProperty[] properties = m_propertiesFlattenedByIndex;
            if (properties == null)
            {
                EnsureFlattenedProperties();
                properties = m_propertiesFlattenedByIndex;
            }
            return properties;
        }

        private void EnsureFlattenedProperties()
        {
            Dictionary<string, ThingProperty> byName = new Dictionary<string, ThingProperty>();

            ThingPropertyContainerMetaData typeData = this;
            while (typeData != null)
            {
                if (typeData.HasProperties)
                {
                    foreach (ThingProperty currProp in typeData.PropertiesAtClassLevel)
                    {
                        byName.Add(currProp.Name, currProp);
                    }
                }
                typeData = typeData.BaseTypeMetaData;
            }

            ThingProperty[] byIndex = new ThingProperty[byName.Count];
            foreach (ThingProperty currProp in byName.Values)
            {
                byIndex[currProp.TypeHeirarchyIndex] = currProp;
            }

            m_propertiesFlattenedByIndex = byIndex;
            m_flattenedPropertiesByName = byName;
        }


        public bool HasProperties
        {
            get
            {
                return m_propertiesAtThisClassLevel != null && m_propertiesAtThisClassLevel.Count > 0;
            }
        }

        /// <summary>
        /// Gets properties from this class and its super classes.
        /// </summary>
        /// <returns></returns>
        public ThingProperty[] AllProperties()
        {
            EnsureFlattenedProperties();
            return m_propertiesFlattenedByIndex;
        }

        /// <summary>
        /// Gets all properties at all class levels that have the given data type.
        /// </summary>
        /// <param name="recordType"></param>
        /// <returns></returns>
        public ThingProperty[] PropertiesPerDataType(Type recordType)
        {
            ThingProperty[] properties;
            if (!_PropertiesPerDataType.TryGetValue(recordType, out properties))
            {
                lock (_PropertiesPerDataType)
                {
                    if (!_PropertiesPerDataType.TryGetValue(recordType, out properties))
                    {
                        properties = AllProperties().Where(p => p.DataModelType != null && p.DataModelType.Equals(recordType)).ToArray();
                        _PropertiesPerDataType.Add(recordType, properties);
                    }
                }
            }
            return properties;
        }

        private static readonly Dictionary<Type, ThingProperty[]> _PropertiesPerDataType = new Dictionary<Type, ThingProperty[]>();



        public IEnumerable<ThingProperty> GetPropertiesFromThisAndBaseClasses(Func<ThingProperty, bool> filter)
        {
            EnsureFlattenedProperties();
            foreach (ThingProperty currProp in m_propertiesFlattenedByIndex)
            {
                if (filter(currProp))
                    yield return currProp;
            }
        }

        public IEnumerable<ThingProperty> GetProperties(Func<ThingProperty, bool> filter)
        {
            foreach (ThingProperty currProp in PropertiesAtClassLevel)
            {
                if (filter(currProp))
                    yield return currProp;
            }
        }


        /// <summary>
        /// Gets the property, at this class level or any super classes, with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ThingProperty Property(string name)
        {
            EnsureFlattenedProperties();
            ThingProperty prop;
            if (m_flattenedPropertiesByName.TryGetValue(name, out prop))
                return prop;
            return null;
        }

        public ThingProperty GetPropertyByHeriarchyIndex(int index)
        {
            EnsureFlattenedProperties();
            return m_propertiesFlattenedByIndex[index];
        }


        /// <summary>
        /// Called by ThingProperty.Register.  Do not call elsewhere.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal int AddProperty(ThingProperty property)
        {
            if (m_propertiesAtThisClassLevel == null)
            {
                m_propertiesAtThisClassLevel = new List<ThingProperty>();
            }
            m_propertiesAtThisClassLevel.Add(property);

            return m_propertiesAtThisClassLevel.Count - 1;
        }

        /// <summary>
        /// Gets the metadata for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ThingPropertyContainerMetaData GetMetaData(Type type)
        {
            ThingPropertyContainerMetaData data;
            if (!m_typeMetaDatas.TryGetValue(type, out data))
            {
                lock (m_typeMetaDatas)
                {
                    if (!m_typeMetaDatas.TryGetValue(type, out data))
                    {
                        data = CreateMetaData(type);
                        if (data == null)
                            return null;
                        m_typeMetaDatas.Add(type, data);
                    }
                }
            }
            return data;

        }

        private static ThingPropertyContainerMetaData CreateMetaData(Type type)
        {
            if (type.Assembly.Equals(typeof(System.Object).Assembly))
                return null;
            var componentType = new ThingPropertyContainerMetaData
            {
                Type = type
            };

            var baseType = type.BaseType;
            if (!baseType.Assembly.Equals(typeof(System.Object).Assembly))
            {
                componentType.BaseTypeMetaData = GetMetaData(baseType);
            }

            return componentType;

        }

        private static Dictionary<Type, ThingPropertyContainerMetaData> m_typeMetaDatas = new Dictionary<Type, ThingPropertyContainerMetaData>();


    }



}
