using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;

namespace System
{
    /// <summary>
    /// Various utils for json.  
    /// </summary>
    public static class Json
    {

        //public static byte[] SerializeToBytes(object value)
        //{
        //    var jsonString = SerializeToString(value);

        //    return JsonSerializer.SerializeToString(value, value.GetType());
        //}

        public static string Serialize(object value)
        {
            return JsonSerializer.SerializeToString(value,value.GetType());
        }

        public static T Deserialize<T>(string json)
        {
            return (T)JsonSerializer.DeserializeFromString(json, typeof(T));
        }

    }
}
