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
//    public class SendDigestEmailsOptions
//    {
//        [Option("d", "dryrun", Required = false, HelpText = "When specified, no actual emails will be sent.")]
//        public bool DryRun;

//        [Option("l", "limitsenders", Required = false, HelpText = "Specifies a comma-separated list of emails that will be sent to.  All others will be ignored.  For testing.")]
//        public string LimitSendersTo;

//    }

//    public class SendDigestEmails : BaseCommandWithOptions<SendDigestEmailsOptions>
//    {
//        private Dictionary<int, ChangesForUser> DigestContentsPerUser;
//        private List<ProjectThing> ProductionsWithChanges;
//        private DateTime EarliestPossibleChangeFrom;
//        private DateTime SendTime;
//        private static TimeSpan MaxPeriod = TimeSpan.FromDays(3);
//        /// <summary>
//        /// The number of minutes that are allowed.  So, an hourly email can actually be sent 55 min prior.  This helps keep sending times relatively constant.
//        /// </summary>
//        private const int MinutesOfBufferTime = 5;

//        static int EarlyHour = 8;//8am
//        static int LateHour = 16;//4pm
//        static int DailyHour = 6; //send daily emails out in the 6am hour, so time period is 6am yesterday to 5:59.999 today

//        /// <summary>
//        /// Stores the changes for a single production for a single recipient.
//        /// </summary>
//        private class ProductionChanges
//        {
//            public ProductionChanges(ProjectThing production)
//            {
//                Production = production;
//                Changes = new List<ChangeDigestEmailItem>();
//            }
//            public ProjectThing Production { get; private set; }

//            public List<ChangeDigestEmailItem> Changes { get; private set; } 
//        }

//        class ChangesForUser
//        {
//            public ChangesForUser(UserThing user, DateTime periodFrom, DateTime periodTo)
//            {
//                User = user;
//                PeriodFrom = periodFrom;
//                PeriodTo = periodTo;
//                Productions = new List<ProductionChanges>();
//            }

//            public UserThing User { get; private set; }

//            /// <summary>
//            /// Set when this is first created.
//            /// </summary>
//            public DateTime PeriodFrom { get; private set; }

//            /// <summary>
//            /// Set when this is first created.
//            /// </summary>
//            public DateTime PeriodTo { get; private set; }

//            public List<ProductionChanges> Productions { get; private set; } 
//        }

//        public SendDigestEmails()
//        {
//            ProductionsWithChanges = new List<ProjectThing>();
//            DigestContentsPerUser = new Dictionary<int, ChangesForUser>();
//            EnableXmlReport = true;
//            NotifyOnErrors = true;
//            LogJobInDatabase = true;      
//        }

//        protected override void Perform(SendDigestEmailsOptions arguments)
//        {
//            Step("PrepareApp", PrepareApp);

//            SendTime = StartedOn.Value;
//            EarliestPossibleChangeFrom = SendTime.Subtract(MaxPeriod);
//            AddReportAttribute("EarliestPossibleChange", EarliestPossibleChangeFrom.ToString(DateTimeFormat.ShortDateTime));
//            WriteLine("Sending digest emails for time period {0} - {1}", false, EarliestPossibleChangeFrom.ToString(DateTimeFormat.ShortDateTime), SendTime.ToString(DateTimeFormat.ShortDateTime));

//            Step("GetProductionsWithChanges", GetProductionsWithChanges);

//            //all done!
//            if (!ProductionsWithChanges.HasItems())
//            {
//                WriteLine("No productions with changes in the time period.  Job Done.", true);
//                return;
//            }

//            Step("FindPotentialUsersToSendTo", FindPotentialUsersToSendTo);

//            Step("FindDefiniteUsersToSendTo", FindDefiniteUsersToSendTo);

//            Step("SendEmails", SendEmails);
//        }

//        public static void PrepareApp()
//        {
//            ApplicationSettings.EnableDeferredRequests = false;
//            ApplicationSettings.UsePrecompiledViewEngine = true;
//            IoC.Bind<IAppUrlPrefixes>().To<EmailUrlPrefixes>().InSingletonScope();

//            //no application_start here so we use webactivator
//            WebActivator.ActivationManager.Run();
//        }


//        /// <summary>
//        /// Fills ProductionsWithChanges with all productions that have changes within the allowable period.
//        /// </summary>
//        void GetProductionsWithChanges()
//        {
//            using (var db = DataModel.ReadOnly)
//            {
//                var potentialProdictions = db.Actions.Join(db.ActionSources, a => a.ChangeDetailsId, a => a.Id,
//                                (Action, ActionSource) => new { Action, ActionSource })
//                    .Join(db.Things, d => d.Action.ThingId, d => d.Id,
//                          (Action, Thing) => new { Action.ActionSource, Thing })
//                    .Where(
//                        d =>
//                        d.ActionSource.Date >= EarliestPossibleChangeFrom && d.ActionSource.Date < SendTime &&
//                        d.Thing.DeletedOn == null &&
//                        d.Thing.Type == Things.ThingType(typeof(ProjectThing)))
//                    .Select(d => d.Thing.Id).Distinct().ToArray().Once((ids) =>
//                    {
//                        WriteLine("Found {0} productions in database with changes in time period.", false, ids == null ? 0 : ids.Length);
//                    }).Select(i => Things.GetOrNull<ProjectThing>(i)).Where(
//                        p => p != null && p.Activity.HasItems()).AddAllTo(ProductionsWithChanges);

//                WriteLine("Found {0} potential productions.", false, ProductionsWithChanges.Count);
//                AddReportAttribute("PotentialProductions", ProductionsWithChanges.Count);
//            }
//        }

//        /// <summary>
//        /// Goes through each production that has changes in our allowable period and fills all members that should recieve change notifications 
//        /// for some subset of the max time period.  After this we have to narrow down to exact changes.
//        /// </summary>
//        private void FindPotentialUsersToSendTo()
//        {
////now go through each and gather changes for individual users
//            foreach (var production in ProductionsWithChanges)
//            {
//                foreach (var memberId in production.GetMembers())
//                {
//                    //add new potential users on demand
//                    if (!DigestContentsPerUser.ContainsKey(memberId))
//                    {
//                        ChangesForUser changes = null;
//                        Step("GetChangePeriodForUser", () => changes = GetChangePeriodForUser(memberId));
//                        DigestContentsPerUser.Add(memberId, changes);
//                        if (changes != null)
//                        {
//                            AddDynamicObjectToReport("PotentialUserToSendTo", new
//                                                                                  {
//                                                                                      UserId = changes.User.Id,
//                                                                                      Email = changes.User.Email,
//                                                                                      PeriodFromUtc = changes.PeriodFrom.ToString(DateTimeFormat.ShortDateTime),
//                                                                                      PeriodToUtc = changes.PeriodTo.ToString(DateTimeFormat.ShortDateTime),
//                                                                                      PeriodFromUserLocal = changes.User.UtcToLocal(changes.PeriodFrom),
//                                                                                      PeriodToUserLocal = changes.User.UtcToLocal(changes.PeriodTo),
//                                                                                  });
//                        }
//                    }
//                }
//            }
//        }


//        /// <summary>
//        /// Fills the changes to include for every user. 
//        /// </summary>
//        private void FindDefiniteUsersToSendTo()
//        {
//            //now go through each and gather changes for individual users
//            foreach (var production in ProductionsWithChanges)
//            {
//                foreach (var memberId in production.GetMembers())
//                {
//                    if (DigestContentsPerUser[memberId] != null)
//                    {
//                        ProcessProductionChangesForUser(production, DigestContentsPerUser[memberId]);
//                    }
//                }
//            }
//        }


//        /// <summary>
//        /// Sends all the emails to each user.
//        /// </summary>
//        private void SendEmails()
//        {
//            HashSet<string> limitedSenders = null;
//            if ( Options.LimitSendersTo.HasChars())
//            {
//                foreach( var email in Options.LimitSendersTo.Split(',').Select(e => e.Trim()).Where(e => e.HasChars()))
//                {

//                    AddDynamicObjectToReport("LimitedSender", new
//                                                                  {
//                                                                      Email = email,
//                                                                  });
//                    if ( limitedSenders == null )
//                        limitedSenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//                    limitedSenders.Add(email);
//                }
//            }
//            foreach (var changesForUser in DigestContentsPerUser.Values)
//            {
//                if (changesForUser != null && changesForUser.Productions.HasItems())
//                {
//                    if (limitedSenders == null || limitedSenders.Contains(changesForUser.User.Email))
//                    {
//                        Step("SendEmail", () => SendEmail(changesForUser));
//                    }
//                }
//            }
//        }

//        private void SendEmail(ChangesForUser changesForUser)
//        {
//            var viewModel = new Email_ActivityDigest
//                                {
//                                    PeriodStart = changesForUser.User.UtcToLocal(changesForUser.PeriodFrom),
//                                    PeriodEnd = changesForUser.User.UtcToLocal(changesForUser.PeriodTo),
//                                    ActivityPerProduction = changesForUser.Productions.OrderBy(p => p.Production.Title).Select(productionChanges =>
//                                                                                                  {
//                                                                                                      var changes = productionChanges.Changes.OrderByDescending(c => c.Date);
//                                                                                                      var change = new Email_ActivityDigest.ProductionActivity()
//                                                                                                      {
//                                                                                                            ProductionId = productionChanges.Production.Id,
//                                                                                                            ProductionTitle = productionChanges.Production.Title,
//                                                                                                            ProductionUrl = ProductionUrl(productionChanges.Production),
//                                                                                                            PlainTextChanges = changes.Select(c => c.Text).ToArray(),
//                                                                                                            HtmlChanges = changes.Select(c => c.Html).ToArray(),
//                                                                                                      };
//                                                                                                      productionChanges.Production.GetLogo().IfNotNull(l =>
//                                                                                                                                                 {
//                                                                                                                                                     change.LogoUrl = l.GetUrlHttps();
//                                                                                                                                                     change.LogoHeight = l.Height;
//                                                                                                                                                     change.LogoWidth = l.Width;
//                                                                                                                                                 });
//                                                                                                      return change;
//                                                                                                  }).ToArray(),
//                                };
//            //get the email html and plain text
//            var mail = new EmailRenderer
//                               {
//                                   Model = viewModel,
//                                   View = "ActivityDigest"
//                               }.Render();
//            mail.To = changesForUser.User.Email;
//            mail.ToUserId = changesForUser.User.Id;
//            mail.Template = "ActivityDigest";
//            mail.JobId = JobId;
//            mail.Id = Guid.NewGuid();
      
//                this.AddDynamicObjectToReport("SendEmail",
//                    new
//                        {
//                            UserId = changesForUser.User.Id,
//                            changesForUser.User.Email,
//                            MailId = mail.Id,
//                        });

//            if ( Options.DryRun)
//            {
//                OutgoingMail.LogMail(mail);
//            }
//            else
//            {
//                OutgoingMail.Send(mail);

//                //record it
//                var email = new Models.Data.ActivitySummaryEmail
//                {
//                    SentOn = DateTime.UtcNow,
//                    PeriodStart = changesForUser.PeriodFrom,
//                    PeriodEnd = changesForUser.PeriodTo,
//                    UserId = changesForUser.User.Id,
//                    JobId = JobId,
//                };
//                using (var dataModel = new DataModel())
//                {
//                    dataModel.ActivitySummaryEmails.InsertOnSubmit(email);
//                    dataModel.SubmitChanges();
//                }                
//            }
//        }

//        /// <summary>
//        /// Gets the change period information for the given user.  Returns null if the time period is invalid or the user can't be found.
//        /// </summary>
//        /// <param name="userId"></param>
//        /// <returns></returns>
//        ChangesForUser GetChangePeriodForUser(int userId)
//        {
//            AddReportAttribute("UserId", userId);
//            var user = Things.GetOrNull<UserThing>(userId);
//            if (user == null || !user.Email.IsValidEmail())
//                return null;

//            var strDigestFrequency = (string)user.GetSettingValue(UserSettings.DigestEmailFrequency.Name);
//            EmailDigestFrequency digestFrequency;
//            AddReportAttribute("DigestSetting", strDigestFrequency);
//            if (!strDigestFrequency.HasChars() || !Enum.TryParse(strDigestFrequency, true, out digestFrequency) || digestFrequency == EmailDigestFrequency.Never)
//            {
//                return null;
//            }

//            var lastEmailPeriodEnd = new DateTime?();
//            using (var db = DataModel.ReadOnly)
//            {
//                var latestEmail =
//                    (from dm in db.ActivitySummaryEmails
//                     orderby dm.Id descending
//                     where dm.UserId == userId
//                     select new { dm.PeriodEnd }).FirstOrDefault();
//                if (latestEmail != null)
//                {
//                    lastEmailPeriodEnd = latestEmail.PeriodEnd;
//                    AddReportAttribute("LastEmailPeriodEnd", lastEmailPeriodEnd.GetValueOrDefault().ToString(DateTimeFormat.ShortDateTime));
//                    if ( EarliestPossibleChangeFrom > lastEmailPeriodEnd )//if for some reason they haven't recieved the email in a while, make the lower bound equal to the job's limit.  this avoids including old changes in recent emails
//                    {
//                        lastEmailPeriodEnd = EarliestPossibleChangeFrom;
//                        AddReportAttribute("ModifiedLastEmailPeriodEnd", lastEmailPeriodEnd);
//                    }
//                }
//            }
//            var nowUserLocal = user.UtcToLocal(SendTime);
//            AddReportAttribute("UserLocalSendTime", nowUserLocal.ToString(DateTimeFormat.ShortDateTime));
//            //basically, if the desired time period has passed since their last email, do it.  
//            ChangesForUser result = null;
//            switch (digestFrequency)
//            {
//                case EmailDigestFrequency.Hourly:
//                    if (!lastEmailPeriodEnd.HasValue)
//                    {
//                        result = new ChangesForUser(user, SendTime.Subtract(TimeSpan.FromHours(1)), SendTime);
//                    } 
//                    else if ( (SendTime - lastEmailPeriodEnd.Value).TotalMinutes < 55 )
//                    {
//                        AddReportAttribute("IgnoreBecause", "TooSoon");
//                        break;//wait till next time.  task ran a bit too soon
//                    }
//                    else
//                    {
//                        result = new ChangesForUser(user, lastEmailPeriodEnd.Value, SendTime);                        
//                    }
//                    break;
//                case EmailDigestFrequency.BeforeDuringWorkDay:

//                    if (lastEmailPeriodEnd.HasValue && (SendTime - lastEmailPeriodEnd.Value).TotalHours <= 3)//in case it was run any time in the last few hours, wait till next opportunity
//                    {
//                        AddReportAttribute("IgnoreBecause", "TooSoon");
//                    } 
//                    else if (nowUserLocal.Hour == EarlyHour)//it's the 8am hour, so time period is 4pm yesterday until 7:59.999am today
//                    {
//                        result = new ChangesForUser(user, user.LocalToUtc(nowUserLocal.Date.AddDays(-1).AddHours(LateHour)), SendTime);
//                    }
//                    else if (nowUserLocal.Hour == LateHour)//it's the 4pm hour so the target time period is 8am - 3:59.999
//                    {
//                        result = new ChangesForUser(user, user.LocalToUtc(nowUserLocal.Date.AddHours(EarlyHour)), SendTime);
//                    }
//                    else
//                    {
//                        AddReportAttribute("IgnoreBecause", "WrongHour");                        
//                    }

//                    break;
//                case EmailDigestFrequency.Daily:

//                    if (lastEmailPeriodEnd.HasValue && (SendTime - lastEmailPeriodEnd.Value).TotalHours <= 12)//in case it was run any time in the last few hours, wait till next opportunity
//                    {
//                        AddReportAttribute("IgnoreBecause", "TooSoon");
//                    }
//                    else if (nowUserLocal.Hour == DailyHour)
//                    {
//                        result = new ChangesForUser(user, user.LocalToUtc(nowUserLocal.Date.AddDays(-1).AddHours(DailyHour)), SendTime);
//                    }
//                    else
//                    {
//                        AddReportAttribute("IgnoreBecause", "WrongHour");                                                
//                    }
//                    break;
//            }
//            if (result != null)
//            {
//                AddReportAttribute("StartPeriodUtc", result.PeriodFrom.ToString(DateTimeFormat.ShortDateTime));
//                AddReportAttribute("StartPeriodLocal", user.UtcToLocal(result.PeriodFrom).ToString(DateTimeFormat.ShortDateTime));
//                AddReportAttribute("EndPeriodUtc", result.PeriodTo.ToString(DateTimeFormat.ShortDateTime));
//                AddReportAttribute("EndPeriodLocal", user.UtcToLocal(result.PeriodTo).ToString(DateTimeFormat.ShortDateTime));
//            }
//            return result;
//        }

//        public static string ProductionUrl(ProjectThing production)
//        {
//            if (production == null)
//                throw new ArgumentNullException("production");
//            string relativeUrl;
//            if (production.VanityUrl.HasChars())
//            {
//                relativeUrl = production.VanityUrl;
//            }
//            else
//            {
//                relativeUrl = "productions/" + production.Id.ToInvariant();
//            }
//            return IoC.Get<IAppUrlPrefixes>().FromRelative(relativeUrl, UrlType.AbsoluteHttps);
//        }


//        /// <summary>
//        /// Adds changes for a particular production for a particular member
//        /// </summary>
//        /// <param name="production"></param>
//        /// <param name="changesForUser"></param>
//        void ProcessProductionChangesForUser(ProjectThing production, ChangesForUser changesForUser)
//        {
//            ProductionChanges productionChanges = null;
//            //only consider activity within the user's window
//            foreach (var activity in production.Activity.Where(a => a.Date >= changesForUser.PeriodFrom && a.Date < changesForUser.PeriodTo))
//            {
//                ThingBase changedThing = activity.ThingId == production.Id ? production : production.FindDescendantById(activity.ThingId);
//                //in this case the thing was deleted.  so we ignore the change.  the Delete change will maybe be included in the change set or a ancestor was delted
//                if (changedThing == null)
//                    continue;

//                var emailItem = changedThing.GetActivityEmailContents(activity, changesForUser.User);
//                if ( emailItem != null )
//                {
//                    if (productionChanges == null)
//                    {
//                        productionChanges = new ProductionChanges(production).AddTo(changesForUser.Productions);

//                        AddDynamicObjectToReport("FoundChanges", new
//                        {
//                            ProductionId = production.Id,
//                            changesForUser.User.Email,
//                            UserId = changesForUser.User.Id,
//                        });
//                    }
//                    productionChanges.Changes.Add(emailItem);
//                }
//            }
            
//        }

//    }


//}

