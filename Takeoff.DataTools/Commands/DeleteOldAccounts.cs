using System;
using System.Linq;
using CommandLine;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{
    public class DeleteOldAccountsOptions
    {
        [Option('t', "trialdays", Required = true, HelpText = "Number of days for expired trials before they are deleted.")]
        public int TrialExpiredDaysToDelete { get; set; }

        [Option('d', "demodays", Required = true, HelpText = "Number of days for demos before they are deleted.")]
        public int DemoDaysToDelete { get; set; }

    }

    public class DeleteOldAccounts : BaseCommandWithOptions<DeleteOldAccountsOptions>
    {
        public DeleteOldAccounts()
        {
            EnableXmlReport = true;
            NotifyOnErrors = true;
            LogJobInDatabase = true;
        }


        protected override void Perform(DeleteOldAccountsOptions arguments)
        {
            if (arguments.TrialExpiredDaysToDelete < 10)
                throw new Exception("No way dude.  TrialExpiredDaysToDelete must be 10 or more.");
            var dataModel = new DataModel();

            Step("DeleteExpiredTrials", () =>
            {
                var accountIds = (from account in dataModel.Accounts
                              join accountThing in dataModel.Things on account.ThingId equals accountThing.Id
                              where
                                  accountThing.DeletedOn == null && account.Status == AccountStatus.TrialExpired.ToString() &&
                                  account.TrialPeriodEndsOn != null &&
                                  account.TrialPeriodEndsOn.Value.Date < DateTime.UtcNow.Date.Subtract(TimeSpan.FromDays(arguments.TrialExpiredDaysToDelete))
                              select accountThing.Id).ToArray();
                this.AddReportAttribute("IdCount", accountIds.Count());
                foreach (var accountId in accountIds)
                {
                    Step("DeleteAccount", true, () => DeleteAccount(accountId, false));//leave user so they can sign back in later on
                }
            });

            Step("DeleteOldDemos", () =>
            {
                var accountIds = (from account in dataModel.Accounts
                                  join accountThing in dataModel.Things on account.ThingId equals accountThing.Id
                                  where
                                      accountThing.DeletedOn == null && account.Status == AccountStatus.Demo.ToString() &&
                                      accountThing.CreatedOn.Date < DateTime.UtcNow.Date.Subtract(TimeSpan.FromDays(arguments.DemoDaysToDelete))
                                  select accountThing.Id).ToArray();
                this.AddReportAttribute("IdCount", accountIds.Count());
                foreach (var accountId in accountIds)
                {
                    Step("DeleteAccount", true, () => DeleteAccount(accountId, true));
                }
            });

            ////sometimes (perhaps because Account delete is busted), productions whose account is deleted aren't delete themselves, which means we can't physically delete the files, which costs money.  this fixes that
            //Step("DeleteOrphanedProductions", () =>
            //{

            //    var productionIds = (from project in dataModel.Projects
            //                         join projectThing in dataModel.Things on project.ThingId equals projectThing.Id
            //                         join accountThing in dataModel.Things on projectThing.AccountId equals accountThing.Id
            //                         where projectThing.DeletedOn == null && accountThing.DeletedOn != null
            //                         select projectThing.Id).ToArray();
            //    this.AddReportAttribute("IdCount", productionIds.Count());
            //    if (productionIds.HasItems())
            //    {
            //        foreach (var productionId in productionIds)
            //        {
            //            Step("DeleteProduction", true, () =>
            //                                               {
            //                                                   var production = Things.Get<ProjectThing>(productionId);
            //                                                   this.AddReportAttribute("Id", productionId);
            //                                                   production.Delete();
            //                                               }
            //                );
            //        }
            //    }
            //});
        }

        private void DeleteAccount(int accountId, bool deleteUserRecord)
        {
//todo: send an email
            var account = Things.Get<AccountThing>(accountId);
            var owner = account.Owner;
            AddReportAttribute("AccountId", account.Id);
            AddReportAttribute("OwnerId", owner.Email);
            AddReportAttribute("OwnerEmail", owner.Email);
            Accounts.DeleteAccountAndMaybeUser(owner, DateTime.UtcNow, deleteUserRecord);
        }
    }


}

