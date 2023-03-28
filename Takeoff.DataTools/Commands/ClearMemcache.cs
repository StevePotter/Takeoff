using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{

    public class ClearMemcacheCommand : BaseCommand
    {
        protected override void Perform(string[] commandLineArgs)
        {
            CacheUtil.ClearAppCache();            
        }
    }
}
