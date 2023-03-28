using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Takeoff;
using Takeoff.Data;
using Takeoff.Models;
using ServiceStack.Redis;
using System.Text;

namespace Takeoff
{
    //TODO: check whether caching is enabled for this app (Mule shuldn't cache because it doesn't get invalidation notifcations)
    //when you go memcached, remember that all apps will share the cache, so maybe prepend appname 

    public static class CacheUtil
    {
        const string ContextCacheKey = "__cc";

        public static IThreadCacheService ThreadCache
        {
            get
            {
                if (_ThreadCache == null)
                    _ThreadCache = IoC.Get<IThreadCacheService>();
                return _ThreadCache;
            }
        }
        public static IThreadCacheService _ThreadCache;

        const string ContextPrefix = "_c.";


        private static string GetContextKey(string cacheKey)
        {
            return ContextPrefix + cacheKey;
        }

        public static object GetFromContext(string cacheKey)
        {
            return ThreadCache.Items[GetContextKey(cacheKey)];
        }

        public static void SetInContext(string cacheKey, object value)
        {
            ThreadCache.Items[GetContextKey(cacheKey)] = value;
        }

        public static void RemoveFromContext(string cacheKey)
        {
            ThreadCache.Items.Remove(GetContextKey(cacheKey));
        }

        public static void ClearAppCache()
        {
            DistributedCache.Current.ClearAppCache();
        }

        public static string GetValueFromAppCache(string cacheKey)
        {
            return DistributedCache.Current.GetValueFromAppCache(cacheKey);
        }

        public static string[] GetValuesFromAppCache(IEnumerable<string> cacheKeys)
        {
            return DistributedCache.Current.GetValuesFromAppCache(cacheKeys);
        }

        public static bool SetAppCacheValue(string key, string value)
        {
            return DistributedCache.Current.SetAppCacheValue(key, value);
        }

        public static void RemoveFromAppCache(string cacheKey)
        {
            DistributedCache.Current.RemoveFromAppCache(cacheKey);
        }

        /// <summary>
        /// Memcached doesn't allow spaces and other characters in the cache key.  This avoids that.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public static string SafeCacheKey(string cacheKey)
        {
            return new string(cacheKey.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '@').ToArray());
        }

        public static T GetCachedWithStringSerialization<T>(string cacheKey, bool useContextCache,  Func<T> createCallback, Func<T,string> serialize = null, Func<string,T> deserialize = null)
        {
            if (cacheKey.Contains(' '))
                throw new ArgumentException("cacheKey cannot contain a space");

            Args.NotNull(cacheKey, "cacheKey");
            Args.NotNull(createCallback, "createCallback");

            if (serialize == null)
                serialize = (value) => Json.Serialize(value);
            if (deserialize == null)
                deserialize = (json) => Json.Deserialize<T>(json);

            object fromCache = null;
            if (useContextCache)
            {
                fromCache = GetFromContext(cacheKey);
                if (fromCache != null)
                    return (T)fromCache;
            }

            //now check app-wide cache
            fromCache = GetValueFromAppCache(cacheKey);
            if (fromCache != null && fromCache is String)
            {
                fromCache = deserialize((string)fromCache);
                if (useContextCache)
                    SetInContext(cacheKey, fromCache);
                return (T)fromCache;
            }

            var result = createCallback();
            if (useContextCache)
                SetInContext(cacheKey, result);

            SetAppCacheValue(cacheKey, serialize(result));
            return result;
        }



        private static bool NeedToSerialize<T>()
        {
            return Type.GetTypeCode(typeof(T)) == TypeCode.Object; 
        }

        public static byte[] Serialize(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
        // Convert a byte array to an Object
        public static Object Deserialize(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }



    }
}
