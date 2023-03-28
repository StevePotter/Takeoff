//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data.Entity;
//using System.Data.Entity.ModelConfiguration;
//using System.Data.Linq;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using CommandLine;
//using Takeoff.Data;
//using Takeoff.DataTools.Commands.DataFillerModels;
//using Takeoff.Models;
//using Takeoff.Models.Data;
//using System.IO;

//namespace Takeoff.DataTools.Commands
//{

//    public class RefreshBlogAndTwitter : BaseCommand
//    {
//        public RefreshBlogAndTwitter()
//        {
//            EnableXmlReport = true;
//            NotifyOnErrors = true;
//            LogJobInDatabase = true;
//        }

//        protected override void Perform(string[] commandLineArgs)
//        {
//            Step("RefreshBlog", () => BlogItemRssRepository.RefreshEntries());
//            Step("RefreshTweets", () => TweetsRepository.RefreshEntries());
//        }


//    }

//}
