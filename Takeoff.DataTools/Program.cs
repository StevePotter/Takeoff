using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Takeoff.Data;
using Takeoff.DataTools.Commands;
using Takeoff.Models;
using Takeoff.ThingRepositories;

namespace Takeoff.DataTools
{
    /// <summary>
    /// The program works by getting the command to run from the first command line arg.  The name is 
    /// matched to a command class implemented in the assembly.  This way you can run a bunch 
    /// of different jobs from the same exe.
    /// </summary>
    /// <remarks>TODO: make it easy to create a file of the console output.</remarks>
    class Program
    {
        static int Main(string[] args)
        {
            //get all the commands from the assembly
            var commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var type in typeof(Program).Assembly.GetTypes().Where(t => typeof(BaseCommand).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    commands.Add(type.Name.EndWithout("Command", StringComparison.OrdinalIgnoreCase), type);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach( var typeLoaderEx in ex.LoaderExceptions)
                {
                    Console.WriteLine(typeLoaderEx.Report("Load Exception:"));
                }
                throw;
            }
            //get the command from the arg
            if (!args.HasItems())
            {
                Console.WriteLine("Error: No command specified.  Possible commands are: " + string.Join(",", commands.Keys));
                return 1;
            }
            Type commandType;
            if ( !commands.TryGetValue(args[0], out commandType))
            {
                Console.WriteLine("Error: Command '{0}' could not be found. Possible commands are: " + string.Join(",", commands.Keys), args[0]);
                return 1;             
            }
            InitializeDataSystem();

            BaseCommand command = (BaseCommand)Activator.CreateInstance(commandType);
            Console.WriteLine("Running command type " + commandType.Name);
            Stopwatch runWatch = Stopwatch.StartNew();
            command.Run(args.Skip(1).ToArray());
            runWatch.Stop();
            Console.WriteLine("Command done. Duration: " + BaseCommand.FormatElapsed(runWatch.Elapsed));
            return 0;
        }

        private static void InitializeDataSystem()
        {
//needed for Things.  Really, all the logic should go in Things.Init
            Things.Init();
            //setup all the important repos
            //threadcache needs to be changed to a dummy one because the default uses httpcontext.items
            IoC.Bind<IThreadCacheService, ThreadCache>(false);
#if DEBUG
            IoC.Bind<IDistributedCache, ProcessBasedDistributedCache>();
#endif
            IoC.Bind<IUsersRepository, UsersThingRepository>();//.To<UsersThingRepository>().InSingletonScope();
            IoC.Bind<IPromptRepository, PromptsThingRepository>();//.To<PromptsThingRepository>().InSingletonScope();
            IoC.Bind<IPlansRepository, PlansL2SRepository>();//.To<PlansL2SRepository>().InSingletonScope();
            IoC.Bind<IAccountsRepository, AccountsThingRepository>();//.To<AccountsThingRepository>().InSingletonScope();
            IoC.Bind<IVideosRepository, VideosThingRepository>();//.To<VideosThingRepository>().InSingletonScope();
        }

    }


    class ThreadCache : IThreadCacheService
    {

        public IDictionary Items
        {
            get { return items; }
        }

        private IDictionary items = new Hashtable();
    }


}
