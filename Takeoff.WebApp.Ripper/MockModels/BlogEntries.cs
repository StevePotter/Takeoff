using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using System.IO;

namespace Takeoff.WebApp.Ripper
{
    class BlogItemsRepository : IBlogEntriesRepository
    {
        class Entry : IBlogEntry
        {
            public string Link { get; set; }
            public string Title { get; set; }
            public DateTime DateWritten { get; set; }
            public string Description { get; set; }
        }
        public IEnumerable<IBlogEntry> Get()
        {
            return new Entry[]
                       {
                           new Entry
                               {
                                   Link = "#",
                                   Title = "Blog Item 1",
                                   DateWritten = DummyData.Date1,
                                   Description =
                                       @"There is no slick way to quickly collaborate with people on some content.  All throughout the day, I have things that I’d like to get feedback on – documents, code, designs, etc.  Normally I use skype or email to transfer and discuss.  Both are a messy way to do it.  I would love a great all that lets me easily share and collaborate on any file."
                               },
                           new Entry
                               {
                                   Link = "#",
                                   Title = "Blog Item 2",
                                   DateWritten = DummyData.Date2,
                                   Description =
                                       @"Enter Checkit.  Checkit is similar to box.net, except it’s lighter and more focused on social sharing.  It is integrated into email clients, web browsers, mobile devices, and desktops.  It can hook up to dropbox, icloud, Google Docs, or regular old files.  It works with Facebook, Google+, and has its own login system."
                               },
                       };
        }
    }

}
