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
    /// A tweet that is being viewed.
    /// </summary>
    [Serializable]
    public class Tweet : ITweet
    {
        /// <summary>
        /// The twitter screenname of the person who wrote it.
        /// </summary>
        public string AuthorScreenName { get; set; }

        /// <summary>
        /// Profile image url of the author.
        /// </summary>
        public string AuthorImageUrl { get; set; }

        /// <summary>
        /// When the tweet was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// HTML body of a tweet.
        /// </summary>
        public string Body { get; set; }

    }

    public class TweetsRepository: ITweetsRepository
    {

        const string TwitterCacheKey = "takeoff_twitter";

        public IEnumerable<ITweet> Get()
        {
            using (MiniProfiler.Current.Step("Get Tweets"))
            {
                return CacheUtil.GetCachedWithStringSerialization(TwitterCacheKey, false, () => DownloadEntries() ?? new Tweet[] { });
            }
        }

        /// <summary>
        /// Refreshes Twitter entry cache.  This should be called by a background job or staff page.
        /// </summary>
        public static void RefreshEntries()
        {
            CacheUtil.RemoveFromAppCache(TwitterCacheKey);
            CacheUtil.SetAppCacheValue(TwitterCacheKey, Json.Serialize(DownloadEntries() ?? new Tweet[] { }));
        }

        /// <summary>
        /// Extracts TwitterItem entries from the Twitter's rss feed.
        /// </summary>
        private static Tweet[] DownloadEntries()
        {
            using (MiniProfiler.Current.Step("Downloading Tweets"))
            {
                try
                {
                    var maxEntries = ConfigurationManager.AppSettings["MaxTweets"].ToInt();
                    var screenName = ConfigurationManager.AppSettings["TweetsScreenName"];

                    var twitter = new TweetSharp.TwitterService();
                    var search = twitter.Search(new TweetSharp.SearchOptions { Q = "from:" + screenName});
                    if (search == null)
                        throw new InvalidOperationException("Twitter service could not be reached.");

                    return search.Statuses.Take(maxEntries).Select(s => new Tweet
                    {
                        Body = s.TextAsHtml,
                        CreatedOn = s.CreatedDate,
                        AuthorScreenName = s.Author.ScreenName,
                        AuthorImageUrl = s.Author.ProfileImageUrl,
                    }).ToArray();
                }
                catch//some kind of network failure or something
                {
                    return null;
                }
            }
        }

    }

    public static class Tweets
    {
        
        /// <summary>
        /// The current news to show.  Taken from our Twitter rss feed.
        /// </summary>
        /// <remarks>
        /// Note that if the news hasn't been cached, it'll get downloaded and put into the cache synchronously.  To avoid this, a background job should call RefreshEntries().
        /// </remarks>
        public static IEnumerable<ITweet> Entries
        {
            get
            {
                return IoC.Get<ITweetsRepository>().Get();
            }
        }
        
    }

}
