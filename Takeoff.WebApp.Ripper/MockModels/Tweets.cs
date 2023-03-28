using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using System.IO;

namespace Takeoff.WebApp.Ripper
{
    class TweetsMockRepository : ITweetsRepository
    {
        class Tweet: ITweet
        {
            public string AuthorScreenName { get; set; }
            public string AuthorImageUrl { get; set; }
            public DateTime CreatedOn { get; set; }
            public string Body { get; set; }
        }

        public IEnumerable<ITweet> Get()
        {
            return new Tweet[]
                       {
                           new Tweet
                               {
                                   AuthorScreenName = "Somebody",
                                   AuthorImageUrl = "face.png",
                                   CreatedOn = DummyData.Date1,
                                   Body =
                                       @"Sharing a file is easy.  If you have an app installed, you can just right click on a file.  "
                               },
                           new Tweet
                               {
                                   AuthorScreenName = "Anybody",
                                   AuthorImageUrl = "face.png",
                                   CreatedOn = DummyData.Date2,
                                   Body =
                                       @"Collaborators can also use email to provide their feedback by replying to an email sent from CheckIt with the file attached."
                               }

                       };
        }
    }

}
