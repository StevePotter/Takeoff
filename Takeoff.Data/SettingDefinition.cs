using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Takeoff.Models
{
    /// <summary>
    /// Defines Takeoff's various user settings.
    /// </summary>
    public class SettingDefinitions
    {

        public static Dictionary<string, SettingDefinition> Definitions
        {
            get {
                return _definitions ?? (_definitions = new Dictionary<string, SettingDefinition>(StringComparer.OrdinalIgnoreCase));
            }
        }
        private static Dictionary<string, SettingDefinition> _definitions;

        public static SettingDefinition AddDefinition(string name, object defaultValue, Type type)
        {
            var definition = new SettingDefinition(name, defaultValue, type);
            Definitions.Add(name, definition);
            return definition;
        }

        public static SettingDefinition AddDefinition(SettingDefinition definition)
        {
            Definitions.Add(definition.Name, definition);
            return definition;
        }

    }

    public class SettingDefinition
    {
        public SettingDefinition()
        {

        }

        public SettingDefinition(string name, object defaultValue, Type type)
        {
            Name = name;
            Default = defaultValue;
            Type = type;
        }

        public string Name { get; set; }

        public Type Type { get; set; }

        public object Default { get; set; }

        public object ConvertFromFormValue(string formValue)
        {
            if (Type == typeof(bool))//asp.net mvc checkbox helper includes "true,false" when a checkbox is checked, and "false" otherwise.
            {
                return formValue.Contains("true") ? true : false;
            }
            else if (Type == typeof(String))
            {
                return formValue;
            }

            return TypeDescriptor.GetConverter(Type).ConvertFromInvariantString(formValue);
        }

        public object ConvertValue(object someValue)
        {
            if (someValue == null)
                return Default;
            var type = someValue.GetType();
            if ( Type.IsAssignableFrom(type))
            {
                return someValue;
            }
            return Convert.ChangeType(someValue, Type);
        }
    }

    //public class SettingDefinition<T>: SettingDefinition
    //{
    //    public T DefaultValue { get; set; }

    //    /// <summary>
    //    /// Returns a properly casted value.
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public T ConvertValue(object value)
    //    {
    //        if (value == null)
    //            return DefaultValue;

    //        if ( value is T )
    //            return 
    //    }
    //}
}