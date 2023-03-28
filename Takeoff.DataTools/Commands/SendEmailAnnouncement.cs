using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CommandLine;
using Takeoff.Controllers;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.ViewModels;
using Takeoff.Views.Email;

namespace Takeoff.DataTools.Commands
{
    public class SendEmailAnnouncementOptions
    {
        [Option("t", "template", Required = false, HelpText = "The template that contains the email content.  This cooresponds to a razor view.")]
        public string Template;

        [Option("s", "setting", Required = false, HelpText = "The name of the email setting that the user must have enabled.")]
        public string Setting;

        //I made this required so mistakes won't be made
        [Option("a","addresses", Required = true, HelpText = "'all' or comma-separated list of emails to restrict sending to.")]
        public string Addresses;
    }

    /// <summary>
    /// Sends a (hopefully) one-time announcement about something, such as a new offer or feature.  
    /// Example: TakeoffDataTools SendEmailAnnouncement --template "Announcement-Feature-Printing" --setting "NotifyForNewFeatures" --addresses "all"
    /// 
    /// </summary>
    public class SendEmailAnnouncement : BaseCommandWithOptions<SendEmailAnnouncementOptions>
    {

        private List<UserThing> Users;

        private SettingDefinitions Setting;

        public SendEmailAnnouncement()
        {
            EnableXmlReport = true;
            NotifyOnErrors = true;
            LogJobInDatabase = true;      
            Users = new List<UserThing>();
        }

        protected override void Perform(SendEmailAnnouncementOptions arguments)
        {
            if ( !arguments.Addresses.HasChars())
            {
                throw new Exception("-a argument missing.");
            }

            if (!arguments.Addresses.Trim().EqualsCaseInsensitive("all"))
            {
                OutgoingMail.LimitOutgoingMailToAddresses = new HashSet<string>(arguments.Addresses.Split(',').Select(s => s.Trim()));
            }

            Step("PrepareApp", PrepareApp);

            if (!SettingDefinitions.Definitions.ContainsKey(arguments.Setting))
            {
                throw new Exception("Setting definition not found.  Possible definitions are: " + string.Join(",", SettingDefinitions.Definitions.Keys));
            }

            Step("GetUsers", GetUsers);

            //all done!
            if (!Users.HasItems())
            {
                WriteLine("No subscribed users.  Job Done.", true);
                return;
            }

            Step("SendEmails", SendEmails);
        }

        public static void PrepareApp()
        {
            ApplicationSettings.EnableDeferredRequests = false;
            ApplicationSettings.UsePrecompiledViewEngine = true;
            IoC.Bind<IAppUrlPrefixes>().To<EmailUrlPrefixes>().InSingletonScope();

            //no application_start here so we use webactivator
            WebActivator.ActivationManager.Run();
        }


        /// <summary>
        /// Fills ProductionsWithChanges with all productions that have changes within the allowable period.
        /// </summary>
        void GetUsers()
        {
            using (var db = DataModel.ReadOnly)
            {
                var userIds =
                    (from userThing in db.Things
                     join user in db.Users on userThing.Id equals user.ThingId
                     select userThing.Id).ToArray();
                foreach( var userId in userIds)
                {
                    var user = Things.GetOrNull<UserThing>(userId);
                    if (user != null && user.Email.HasChars() && user.IsVerified && user.GetSettingValue(Options.Setting).CastTo<bool>())
                    {
                        Users.Add(user);
                    }
                }
                WriteLine("Found {0} users to send to out of {1}.", false, Users.Count, userIds.Length);
                AddReportAttribute("SendToUsers", Users.Count);
                AddReportAttribute("TotalUsers", userIds.Length);
            }
        }

        private void SendEmails()
        {

            foreach (var user in Users)
            {
                //get the email html and plain text
                var mail = new EmailRenderer
                               {
                                   View = Options.Template
                               }.Render();
                mail.To = user.Email;
                mail.ToUserId = user.Id;
                mail.Template = Options.Template;
                mail.JobId = JobId;
                mail.Id = Guid.NewGuid();

                OutgoingMail.Send(mail);
            }
        }


    }


}

