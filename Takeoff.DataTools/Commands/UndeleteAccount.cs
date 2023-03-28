using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using CommandLine;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{
       
    public class UndeleteAccountCommandOptions
    {
        [Option("e", "email", Required = true, HelpText = "Email for the given account holder.")]
        public string Email;

    }

    /// <summary>
    /// This is a temporary command created for a huge delete after deleting beta accts.
    /// </summary>
    public class UndeleteAccountCommand : BaseCommandWithOptions<UndeleteAccountCommandOptions>
    {
        public UndeleteAccountCommand()
        {
            EnableXmlReport = true;
            NotifyOnErrors = true;
            LogJobInDatabase = false;
        }


        protected override void Perform(UndeleteAccountCommandOptions arguments)
        {
            using (var db = new DataModel())
            {
                const int SecondsBeforeAndAfterDeleted = 20;
                List<int> deletedContainerIds = new List<int>();
                var userThing = (from u in db.Users
                                 join ut in db.Things on u.ThingId equals ut.Id
                                 where u.Email == arguments.Email
                                 select ut).FirstOrDefault();
                if (userThing == null)
                {
                    WriteLine("User with email address " + arguments.Email + " not found");//todo: here is a good place to have some return value                    
                    return;
                }
                //some people delete the account but not the user.  this will skip an existing user
                if (!userThing.DeletedOn.HasValue)
                {
                    WriteLine("User wasn't deleted, so only undeleting the account.");
                }
                else
                {
                    WriteLine("Undeleting user {0}" + userThing.Id);
                    deletedContainerIds.Add(userThing.Id);
                }

                var accountThing = (from u in db.Accounts
                                    join ut in db.Things on u.ThingId equals ut.Id
                                    where ut.CreatedByUserId == userThing.Id
                                    select ut).FirstOrDefault();
                if ( accountThing == null)
                {
                    WriteLine("Could not find an account for the user.");
                }
                else if (!accountThing.DeletedOn.HasValue)
                {
                    WriteLine("Account wasn't deleted, so we're good.");                    
                }
                else
                {
                    //deletedon, unfortunately, uses current datetime so it could be slightly different for each record.  annoying.  so this 
                    var minDeleted = accountThing.DeletedOn.Value.AddSeconds(-1 * SecondsBeforeAndAfterDeleted);
                    var maxDeleted = accountThing.DeletedOn.Value.AddSeconds(SecondsBeforeAndAfterDeleted);

                    //todo: should you undelete comments written by this person in productions belonging to others

                    //account and possibly productions
                    var deletedContainers = (from t in db.Things where t.CreatedByUserId == userThing.Id && t.IsContainer && t.DeletedOn != null && t.DeletedOn >= minDeleted && t.DeletedOn <= maxDeleted select t).ToArray();
                    WriteLine("Undeleting {0} container objects.".FormatString(deletedContainers.Count()));
                    foreach (var deletedContainer in deletedContainers)
                    {
                        WriteLine("Undeleting {0} {1}".FormatString(deletedContainer.Type, deletedContainer.Id));
                        deletedContainerIds.Add(deletedContainer.Id);
                    }
                }

                if ( deletedContainerIds.HasItems())
                {
                    Things.Undelete(deletedContainerIds.ToArray());
                }
            }

        }

    }
}

