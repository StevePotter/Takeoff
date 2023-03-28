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
//    public class ProcessExpiringTrialsOptions
//    {


//    }

//    public class ProcessExpiringTrials : BaseCommandWithOptions<ProcessExpiringTrialsOptions>
//    {
//        public DataModel Database;

//        public ProcessExpiringTrials()
//        {
//            EnableXmlReport = true;
//            NotifyOnErrors = true;
//            LogJobInDatabase = true;
//        }

//        protected override void Perform(ProcessExpiringTrialsOptions arguments)
//        {
//            Database = DataModel.ReadOnly;

//            Step("PrepareApp", SendDigestEmails.PrepareApp);

//            Step("WarnExpiringIn10Days", () => WarnExpiringInDays(10));
//            Step("WarnExpiringIn3Days", () => WarnExpiringInDays(3));
//            Step("WarnExpiringInTomorrow", () => WarnExpiringTomorrow());
//            Step("ProcessExpiredTrials", () => ProcessExpiredTrials());
   
//        }

//        private void WarnExpiringInDays(int daysLeft)
//        {
//            var accounts = GetTrialAccountsExpiringInDays(daysLeft);
//            if ( !accounts.HasItems())
//                return;
//            var emailTemplate = string.Format("TrialExpiresIn{0}Days", daysLeft);
//            foreach (var account in accounts)
//            {
//                var owner = account.Owner;
//                if (!HasSentEmail(emailTemplate, owner.Id))
//                {
//                    var mail = new EmailRenderer
//                                   {
//                                       Model = new Email_TrialExpiresInDays
//                                                   {
//                                                       ExpiresOn = owner.UtcToLocal(account.TrialPeriodEndsOn.Value),
//                                                       DaysLeft = daysLeft,
//                                                   },
//                                       View = typeof(Views.Email.TrialExpiresInDays).Name,
//                                   }.Render();
//                    mail.To = owner.Email;
//                    mail.ToUserId = owner.Id;
//                    mail.Template = emailTemplate;
//                    mail.JobId = JobId;

//                    this.AddDynamicObjectToReport("SendEmail",
//                                                  new
//                                                      {
//                                                          UserId = owner.Id,
//                                                          owner.Email,
//                                                          MailId = mail.Id,
//                                                      });
//                    OutgoingMail.Send(mail);
//                }
//            }
//        }

//        private void WarnExpiringTomorrow()
//        {
//            var accounts = GetTrialAccountsExpiringInDays(1);
//            if (!accounts.HasItems())
//                return;

//            var emailTemplate = typeof (Views.Email.TrialExpiresTomorrow).Name;
//            foreach (var account in accounts)
//            {
//                var owner = account.Owner;
//                if (!HasSentEmail(emailTemplate, owner.Id))
//                {
//                    var mail = new EmailRenderer
//                    {
//                        View = emailTemplate,
//                    }.Render();
//                    mail.To = owner.Email;
//                    mail.ToUserId = owner.Id;
//                    mail.Template = emailTemplate;
//                    mail.JobId = JobId;

//                    this.AddDynamicObjectToReport("SendEmail",
//                                                  new
//                                                  {
//                                                      UserId = owner.Id,
//                                                      owner.Email,
//                                                      MailId = mail.Id,
//                                                  });
//                    OutgoingMail.Send(mail);
//                }
//            }
//        }

//        private void ProcessExpiredTrials()
//        {
//            var accountIds = (from account in Database.Accounts
//                              join accountThing in Database.Things on account.ThingId equals accountThing.Id
//                              where
//                                  accountThing.DeletedOn == null && account.Status == AccountStatus.Trial.ToString() &&
//                                  account.TrialPeriodEndsOn != null &&
//                                  account.TrialPeriodEndsOn.Value.Date <=
//                                  DateTime.UtcNow.Date//break it down tothe minute, not just the day
//                              select accountThing.Id).ToArray();
//            var accounts =
//                accountIds.Select(accountId => Things.GetOrNull<AccountThing>(accountId)).Where(
//                    a => a != null && a.Owner != null).ToArray();

//            var emailTemplate = typeof(Views.Email.TrialExpired).Name;
//            foreach(var account in accounts)
//            {
//                account.TrackChanges();
//                account.Status = AccountStatus.TrialExpired;
//                account.Update();

//                var owner = account.Owner;
//                if (!HasSentEmail(emailTemplate, owner.Id))
//                {
//                    var mail = new EmailRenderer
//                                   {
//                                       View = emailTemplate,
//                                   }.Render();
//                    mail.To = owner.Email;
//                    mail.ToUserId = owner.Id;
//                    mail.Template = emailTemplate;
//                    mail.JobId = JobId;
//                    this.AddDynamicObjectToReport("AccountSuspended",
//                                                  new
//                                                      {
//                                                          AccountId = account.Id,
//                                                          UserId = owner.Id,
//                                                          owner.Email,
//                                                          MailId = mail.Id,
//                                                      });
//                    OutgoingMail.Send(mail);
//                }
//            }            
//        }

//        private bool HasSentEmail(string emailTemplate, int userId)
//        {
//            return (from e in Database.OutgoingEmailLogs
//                    where e.ToUserId == userId && e.Template == emailTemplate
//                    select e).Count() > 0;
//        }

//        private AccountThing[] GetTrialAccountsExpiringInDays(int daysLeft)
//        {
//            var accountIds = (from account in Database.Accounts
//                              join accountThing in Database.Things on account.ThingId equals accountThing.Id
//                              where
//                                  accountThing.DeletedOn == null && account.Status == AccountStatus.Trial.ToString() &&
//                                  account.TrialPeriodEndsOn != null &&
//                                  account.TrialPeriodEndsOn.Value.Date ==
//                                  DateTime.UtcNow.Date.Add(TimeSpan.FromDays(daysLeft))
//                              select accountThing.Id).ToArray();
//            var accounts =
//                accountIds.Select(accountId => Things.GetOrNull<AccountThing>(accountId)).Where(
//                    a => a != null && a.Owner != null).ToArray();
//            return accounts;
//        }
//    }


//}

