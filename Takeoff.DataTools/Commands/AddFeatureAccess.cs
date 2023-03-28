using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{
    public class AddFeatureAccessOptions
    {
        [Option('e', "email", Required = true, HelpText = "The email address of the account owner.  Used to find the account.")] 
        public string Email { get; set; }

        [Option('n', "name", Required = true, HelpText = "The feature to grant access for.")]
        public string Name { get; set; }
    }

    public class AddFeatureAccessCommand : BaseCommandWithOptions<AddFeatureAccessOptions>
    {
        protected override void Perform(AddFeatureAccessOptions arguments)
        {
            var user = Repos.Users.GetByEmail(arguments.Email);
            if ( user == null)
            {
                throw new Exception("No user with that email could be found.");
            }

            var account = user.Account.CastTo<AccountThing>();
            if ( account.HasAccessToFeature(arguments.Name))
            {
                throw new Exception("Account already has access to this feature.");
            }
            var featureAccess = new FeatureAccessThing
                                    {
                                        Name = arguments.Name,
                                        LogInsertActivity = false,
                                    };
            account.AddChild(featureAccess);
            featureAccess.Insert();
        }
    }
}
