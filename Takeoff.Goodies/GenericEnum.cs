using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{

    //http://damieng.com/blog/2010/10/17/enums-better-syntax-improved-performance-and-tryparse-in-net-3-5


    /// <summary>
    /// Helper class for parsing and working with enums.  Has advantage of strongly-typed reflection and faster parsing.
    /// Does not have support for flags.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Enum<T> where T : struct
    {
        private static readonly IEnumerable<T> All = Enum.GetValues(typeof(T)).Cast<T>();
        private static readonly Dictionary<string, T> InsensitiveNames = All.ToDictionary(k => Enum.GetName(typeof(T), k).ToLowerInvariant(), v => v, StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, T> SensitiveNames = All.ToDictionary(k => Enum.GetName(typeof(T), k), v => v, StringComparer.Ordinal);
        private static readonly Dictionary<int, T> Values = All.ToDictionary(k => Convert.ToInt32(k));
        private static readonly Dictionary<T, string> Names = All.ToDictionary(k => k, v => v.ToString());

        public static bool IsDefined(T value)
        {
            return Names.Keys.Contains(value);
        }

        public static bool IsDefined(string value)
        {
            return SensitiveNames.Keys.Contains(value);
        }

        public static bool IsDefined(int value)
        {
            return Values.Keys.Contains(value);
        }

        public static IEnumerable<T> GetValues()
        {
            return All;
        }

        public static string[] GetNames()
        {
            return Names.Values.ToArray();
        }

        public static string GetName(T value)
        {
            string name;
            Names.TryGetValue(value, out name);
            return name;
        }

        public static T Parse(string value)
        {
            return Parse(value, false);
        }

        public static T Parse(string value, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value was null or empty.");
            T parsed = default(T);
            if (!(ignoreCase ? InsensitiveNames : SensitiveNames).TryGetValue(value, out parsed))
                throw new ArgumentException(string.Format("'{0}' is not one of the named constants defined for the {1} enumeration", value.CharsOr("[null]"), typeof(T).FullName), "value");
            return parsed;
        }

        public static bool TryParse(string value, out T returnValue)
        {
            return TryParse(value, false, out returnValue);
        }

        public static bool TryParse(string value, bool ignoreCase, out T returnValue)
        {
            return (ignoreCase ? InsensitiveNames : SensitiveNames).TryGetValue(value.ToLowerInvariant(), out returnValue);
        }

        public static T? ParseOrNull(string value)
        {
            return ParseOrNull(value, false);
        }

        public static T? ParseOrNull(string value, bool ignoreCase)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            T foundValue;
            if ((ignoreCase ? InsensitiveNames : SensitiveNames).TryGetValue(value.ToLowerInvariant(), out foundValue))
                return foundValue;

            return null;
        }

        public static T? CastOrNull(int value)
        {
            T foundValue;
            if (Values.TryGetValue(value, out foundValue))
                return foundValue;

            return null;
        }
    }
}
