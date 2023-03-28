//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net.Mail;
//using System.Text;
//using System.Web;
//using System.Web.Mvc;
//using CommandLine;
//using Takeoff.Controllers;
//using Takeoff.Data;
//using Takeoff.Models;
//using Takeoff.ViewModels;
//using Takeoff.Views.Email;

//namespace Takeoff.DataTools.Commands
//{
//    public class SendDailyBusinessEventSummaryOptions
//    {
//        //[Option("d", "dryrun", Required = false, HelpText = "When specified, no actual emails will be sent.")]
//        //public bool DryRun;

//        //[Option("l", "limitsenders", Required = false, HelpText = "Specifies a comma-separated list of emails that will be sent to.  All others will be ignored.  For testing.")]
//        //public string LimitSendersTo;

//    }

//    public class SendDailyBusinessEventSummary : BaseCommandWithOptions<SendDailyBusinessEventSummaryOptions>
//    {
//        public SendDailyBusinessEventSummary()
//        {
//            EnableXmlReport = true;
//            NotifyOnErrors = true;
//            LogJobInDatabase = true;      
//        }



//        protected override void Perform(SendDailyBusinessEventSummaryOptions arguments)
//        {
//            Step("PrepareApp", PrepareApp);

//            /* 1. determine time period to report on
//             * 2. get biz events in time period
//             * 3. group by type
//             * 4. print out, with type headers.  use nice indentation.  plain text is fine.  include links
//             * 5. send to people specified in appsettings or whatever
//             * 
//             */


//        }

//        public static void PrepareApp()
//        {
//            //no application_start here so we use webactivator.  this registers proper repos
//            WebActivator.ActivationManager.Run();
//        }


//    }


//}

