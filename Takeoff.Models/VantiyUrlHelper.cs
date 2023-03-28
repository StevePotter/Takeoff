using System;
using System.Linq;
using System.Text.RegularExpressions;
using Takeoff.Models;

namespace Takeoff
{
    public static class VantiyUrlHelper
    {
        /// <summary>
        /// Applies to videos or productions.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string CacheKeyForCustomUrl(string url)
        {
            return "curl." + CacheUtil.SafeCacheKey(url);
        }

        public static void AddUrlToCache(string url, int id)
        {
            CacheUtil.SetAppCacheValue(CacheKeyForCustomUrl(url), id.ToInvariant());
        }


        public static void ClearUrlFromCache(string url)
        {
            CacheUtil.RemoveFromAppCache(CacheKeyForCustomUrl(url));
        }

        public static bool IsValid(string url)
        {
            if (String.IsNullOrEmpty(url))
                return false;
            return CustomUrlRegex.Match(url).Success;
        }

        private static readonly Regex CustomUrlRegex = new Regex("^(?:(?![^a-zA-Z0-9_-]).)*$");

        public static bool IsUrlTaken(string url)
        {
            return GetObjectForUrl(url) != null;
        }

        public static object GetObjectForUrl(string url)
        {
            var cacheKey = CacheKeyForCustomUrl(Convert.ToString(url));
            var fromApp = CacheUtil.GetValueFromAppCache(cacheKey);
            if ( fromApp != null)
            {
                var id = fromApp.ToString().ToIntTry();
                if ( id.HasValue)
                    return Things.GetOrNull(id.Value);
            }
            //try productions first.  this should be a rare case during normal operation since most productions will be in the cache

            using (var db = DataModel.ReadOnly)
            {
                var id = (from p in db.Projects
                          join t in db.Things on p.ThingId equals t.Id
                          where t.DeletedOn == null && p.VanityUrl == url
                          select t.Id
                ).FirstOrDefault();
                if (id == default(int))
                {
                    id = (from v in db.Videos
                          join t in db.Things on v.ThingId equals t.Id
                          where t.DeletedOn == null && v.CustomUrl == url
                          select t.Id).FirstOrDefault();
                }
                if (id > 0)
                {
                    var thing = Things.GetOrNull(id);
                    if (thing != null)
                        AddUrlToCache(url, id);
                    return thing;
                }
            }

            return null;
        }
    }
}
