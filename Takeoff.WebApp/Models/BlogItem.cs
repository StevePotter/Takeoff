using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Xml.Linq;
using StackExchange.Profiling;
using Takeoff.Data;

namespace Takeoff.Models
{
    /// <summary>
    /// Contains everything needed to display one of our blog posts as a "news" item
    /// </summary>
    public class BlogItem: IBlogEntry
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public DateTime DateWritten { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Downloads blog items from the rss feed.
    /// </summary>
    public class BlogItemRssRepository : IBlogEntriesRepository
    {
        const string BlogCacheKey = "takeoff_blog";

        public IEnumerable<IBlogEntry> Get()
        {
            using (MiniProfiler.Current.Step("Get Blog Entries"))
            {
                return CacheUtil.GetCachedWithStringSerialization(BlogCacheKey, false, () => DownloadEntries() ?? new BlogItem[] { });
            }
        }


        /// <summary>
        /// Refreshes blog entry cache.  This should be called by a background job or staff page.
        /// </summary>
        public static void RefreshEntries()
        {
            CacheUtil.RemoveFromAppCache(BlogCacheKey);
            CacheUtil.SetAppCacheValue(BlogCacheKey, Json.Serialize(DownloadEntries() ?? new BlogItem[] { }));
        }

        /// <summary>
        /// Extracts BlogItem entries from the blog's rss feed.
        /// </summary>
        private static BlogItem[] DownloadEntries()
        {
            using (MiniProfiler.Current.Step("Downloading Blog Items"))
            {
                try
                {
                    var url = ConfigurationManager.AppSettings["BlogRssUrl"];
                    var maxEntries = ConfigurationManager.AppSettings["MaxBlogEntries"].ToInt();
                    var bodyChars = ConfigurationManager.AppSettings["BlogMaxBodyChars"].ToInt();
                    //Downloads the rss feed and fills Http cache with news items.
                    return XDocument.Load(url)
                        .Descendants("item").Take(maxEntries).Select(x => new BlogItem
                        {
                            Link = x.Element("link").Value,
                            Title = x.Element("title").Value,
                            Description = x.Element("description").Value.Truncate(bodyChars, StringTruncating.EllipsisWord),
                            DateWritten = DateTime.Parse(x.Element("pubDate").Value)
                        }).ToArray();
                }
                catch//some kind of network failure or something
                {
                    return null;
                }
            }
        }

    }

    public static class BlogItems
    {
        const string BlogCacheKey = "takeoff_blog";

        /// <summary>
        /// The current news to show.  Taken from our blog rss feed.
        /// </summary>
        /// <remarks>
        /// Note that if the news hasn't been cached, it'll get downloaded and put into the cache synchronously.  To avoid this, a background job should call RefreshEntries().
        /// </remarks>
        public static IEnumerable<IBlogEntry> Entries
        {
            get
            {
                return IoC.Get<IBlogEntriesRepository>().Get();
            }
        }


    }

}
