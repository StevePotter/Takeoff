using ServiceStack.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takeoff.Models
{
    public static class DistributedCache
    {
        public static IDistributedCache Current
        {
            get { return _current.Value; }
        }
        private static Lazy<IDistributedCache> _current = new Lazy<IDistributedCache>(() =>
        {
            return IoC.GetOrNull<IDistributedCache>() ?? new RedisDistributedCache();
        },true);
    }

    public interface IDistributedCache
    {
        void ClearAppCache();
        string GetValueFromAppCache(string cacheKey);
        string[] GetValuesFromAppCache(IEnumerable<string> cacheKeys);
        bool SetAppCacheValue(string key, string value);
        void RemoveFromAppCache(string cacheKey);

    }

    /// <summary>
    /// Distributed cache provider for Redis (default)
    /// </summary>
    public class RedisDistributedCache: IDistributedCache
    {

        private static PooledRedisClientManager RedisClientManager
        {
            get
            {
                if (redisClientPool == null)
                {
                    lock (redisClientPoolLock)
                    {
                        if (redisClientPool == null)
                        {
                            redisClientPool = new PooledRedisClientManager("localhost");
                            //                            redisClientPool.Start();
                        }
                    }
                }
                return redisClientPool;
            }
        }
        private static PooledRedisClientManager redisClientPool;
        private static object redisClientPoolLock = new object();


        public void ClearAppCache()
        {
            using (var redis = RedisClientManager.GetCacheClient())
            {
                redis.FlushAll();
            }
        }

        public string GetValueFromAppCache(string cacheKey)
        {
            using (var redis = RedisClientManager.GetReadOnlyClient())
            {
                var bytes = redis.Get<byte[]>(cacheKey);
                if (bytes == null)
                    return null;
                return Encoding.UTF8.GetString(bytes);
            }
        }

        public string[] GetValuesFromAppCache(IEnumerable<string> cacheKeys)
        {
            using (var redis = RedisClientManager.GetReadOnlyClient())
            {
                List<string> results = new List<string>();
                foreach (var cacheKey in cacheKeys)
                {
                    var bytes = redis.Get<byte[]>(cacheKey);
                    results.Add(bytes == null ? null : Encoding.UTF8.GetString(bytes));
                }
                return results.ToArray();
            }
        }

        public bool SetAppCacheValue(string key, string value)
        {
            using (var redis = RedisClientManager.GetCacheClient())
            {
                var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
                return redis.Set(key, bytes);
            }
        }

        public void RemoveFromAppCache(string cacheKey)
        {
            using (var redis = RedisClientManager.GetCacheClient())
            {
                redis.Remove(cacheKey);
            }
        }

    }

    /// <summary>
    /// Distributed cache provider that uses a static dictionary to hold values.  Useful for development.
    /// </summary>
    public class ProcessBasedDistributedCache : IDistributedCache
    {
        private static ConcurrentDictionary<string, string>  storage = new ConcurrentDictionary<string, string>();
        public void ClearAppCache()
        {
            storage.Clear();
        }

        public string GetValueFromAppCache(string cacheKey)
        {
            string result = null;
            storage.TryGetValue(cacheKey, out result);
            return result;
        }

        public string[] GetValuesFromAppCache(IEnumerable<string> cacheKeys)
        {
            return cacheKeys.Select(k => GetValueFromAppCache(k)).ToArray();
        }

        public void RemoveFromAppCache(string cacheKey)
        {
            string value = null;
            storage.TryRemove(cacheKey, out value);
        }

        public bool SetAppCacheValue(string key, string value)
        {
            var result = storage.ContainsKey(key);
            storage[key] = value;
            return result;
        }
    }
}
