using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Newtonsoft.Json;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{
    public class RemoveGuestAccountsOptions
    {
        [Option('d', "dryrun", Required = false, HelpText = "if true, no changes will be made.")]
        public bool DryRun { get; set; }

    }

    public class RemoveGuestAccounts : BaseCommandWithOptions<RemoveGuestAccountsOptions>
    {
        public RemoveGuestAccounts()
        {
            EnableXmlReport = true;
            NotifyOnErrors = true;
            LogJobInDatabase = true;
        }
        private UserThing[] usersWithAccount;
        private DataModel db;
        private List<UserThing> accountHoldersWithNoProductions;

        protected override void  Perform(RemoveGuestAccountsOptions arguments)
        {
            AddReportAttribute("DryRun", arguments.DryRun);
            db = new DataModel();
            Step("GetUsersWithAccount", () =>
                                            {
                                                usersWithAccount =
                                                    (from u in db.Users select u.ThingId).ToArray().Select(
                                                        u => Things.GetOrNull<UserThing>(u)).Where(
                                                            u =>
                                                            u != null && u.Account != null &&
                                                            u.Account.Status == AccountStatus.Trial).ToArray();
                                            });

            Step("GetAccountHoldersWithoutProductions", GetAccountHoldersWithoutProductions);

            Step("DeleteEmptyAccounts", DeleteAccounts);
            //kill the viewprompt that was initially set up for them
            Step("DeleteTransitionViewPrompts", DeleteViewPrompt);
            db.Dispose();
        }

        private void GetAccountHoldersWithoutProductions()
        {
            accountHoldersWithNoProductions = new List<UserThing>();
            foreach (var user in usersWithAccount)
            {
                var productions = (from t in db.Things
                                   where
                                       t.Type == Things.ThingType(typeof (ProjectThing)) &&
                                       t.AccountId == user.Account.Id
                                   select t.Id).ToArray().Select(u => Things.GetOrNull<ProjectThing>(u)).Where(
                                       p => p != null).ToList();
                var nonSamples = productions.Where(p => !p.IsSample).ToArray();
                Console.WriteLine("{0} {1} {2} {3}", user.Email, user.Id, productions.Count(), nonSamples.Count());
                if (nonSamples.Count() == 0)
                {
                    accountHoldersWithNoProductions.Add(user);
                    AddDynamicObjectToReport("UserWithEmptyAccount", new
                                                                         {
                                                                             user.Id,
                                                                             AccountId = user.Account.Id,
                                                                             user.Email,
                                                                         });
                }
            }


        }

        private void DeleteAccounts()
        {
            foreach( var user in accountHoldersWithNoProductions)
            {
                Step("DeleteAccount", () =>
                                          {
                                              DeleteAccount(user);
                                          });
            }
        }

        private void DeleteAccount(UserThing user)
        {
            AddReportAttribute("UserId", user.Id);
            AddReportAttribute("Email", user.Email);
            if ( !Options.DryRun)
                Accounts.DeleteAccountAndMaybeUser(user, DateTime.UtcNow, false);
        }

        private void DeleteViewPrompt()
        {
            foreach( var user in accountHoldersWithNoProductions)
            {
                if ( user.HasPendingPrompts)
                {
                    foreach( var prompt in user.PendingPrompts.ToArray())
                    {
                        if ( !Options.DryRun)
                            Repos.Prompts.Delete(prompt);
                    }
                }
            }
        }

        
    }
}
