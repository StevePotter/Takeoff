using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{
    public class AddViewPromptOptions
    {
        [Option('v', "view", Required = true, HelpText = "The name of the view to add.")] 
        public string View { get; set; }

        [Option('s', "starts")]
        public DateTime? StartsOn { get; set; }

        [Option('e', "expires")]
        public DateTime? ExpiresOn { get; set; }

        [Option('u',"userid",HelpText = "The integer ID of the user to send to.")]
        public int? UserId { get; set; }
    }

    public class AddViewPromptCommand : BaseCommandWithOptions<AddViewPromptOptions>
    {
        protected override void Perform(AddViewPromptOptions arguments)
        {
            var testPromptUser = Things.Get<UserThing>(arguments.UserId.Value);
            var prompt = Repos.Prompts.CreateInstance();
            prompt.View = arguments.View;
            prompt.ExpiresOn = arguments.ExpiresOn;
            prompt.StartsOn = arguments.StartsOn;
            prompt = Repos.Prompts.Add(prompt, testPromptUser);
        }
    }
}
