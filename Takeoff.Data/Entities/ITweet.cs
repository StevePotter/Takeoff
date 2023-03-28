using System;
using System.IO;

namespace Takeoff.Data
{
    /// <summary>
    /// Used on home page to show latest twitter entries.
    /// </summary>
    public interface ITweet
    {
        /// <summary>
        /// The twitter screenname of the person who wrote it.
        /// </summary>
        string AuthorScreenName { get; set; }

        /// <summary>
        /// Profile image url of the author.
        /// </summary>
        string AuthorImageUrl { get; set; }

        /// <summary>
        /// When the tweet was created.
        /// </summary>
        DateTime CreatedOn { get; set; }

        /// <summary>
        /// HTML body of a tweet.
        /// </summary>
        string Body { get; set; }
    }

}