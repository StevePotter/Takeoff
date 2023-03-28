using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CommandLine;
using Takeoff.Controllers;
using System.IO;
using System.Web;
using Moq;
using System.Security.Principal;
using System.Collections;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Specialized;
using System.Web.WebPages;
using System.Web.Hosting;
using System.Reflection;
using Ninject;
using System.Threading;
using System.Linq.Expressions;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Resources;
using Takeoff.ThingRepositories;
using Takeoff.ViewModels;

namespace Takeoff.WebApp.Ripper
{
    public class Options
    {
        [Option("o", "outputpath", Required = true, HelpText = "The directory to drop files into.")]
        public string OutputPath;

        [Option("a", "apppath", Required = true, HelpText = "The directory to the web app root.  Used for copying assets and whatnot.")]
        public string AppDirectory;

        [Option("u", "assetprefix", Required = false, HelpText = "The prefix to apply to all asset urls.")]
        public string AssetPrefix;

        [Option("f", "copyfolders", Required = false, HelpText = "The folders that will get their entire contents copied.  They should be comma separated.")]
        public string FoldersToCopy;

//        [Option("d", "deletehtml", Required = false, HelpText = "Indicates whether any html files in the .")]
//        public bool DeleteHtmlViewNotInOutput;
    }


    /*
     * todo: generate log, including exceptions
     * use assetbooster to copy local assets
     * report exceptiosn in console output
     * include native url in comments on page
     * generate shell to see all pages in a TOC
     * action-less views, like demo limit reached
     * use robocopy with html files
     */
    partial class Program
    {
        public static string OutputPath;// = @"C:\Users\Steve\Dropbox\Takeoff Design\Latest Rips";
        public static string AppDirectory;// = @"C:\Users\Steve\Dropbox\Takeoff Dev\Code\branches\Cheddar1\Takeoff.WebApp\";
        public static string TempHtmlDirectory;

        public static string CurrentActionName { get; set; }
        public static string CurrentControllerName { get; set; }

        public static string AssetPrefix { get; set; }

        static void Main(string[] args)
        {
            var options = new Options();;;
            ICommandLineParser parser = new CommandLineParser();
            if (!parser.ParseArguments(args, options))
            {
                Console.WriteLine("Couldn't parse command line args.");
                return;
            }
            OutputPath = options.OutputPath;
            AppDirectory = options.AppDirectory;
            AssetPrefix = options.AssetPrefix.CharsOrEmpty();

            Console.WriteLine("Takeoff App Ripper");
            Console.WriteLine("App folder: " + AppDirectory);
            Console.WriteLine("Output folder: " + OutputPath);

            Stopwatch w = Stopwatch.StartNew();

            ApplicationSettings.EnableDeferredRequests = false;
            //no application_start here so we use webactivator
            WebActivator.ActivationManager.RunPreStartMethods();
            WebActivator.ActivationManager.RunPostStartMethods();

            IoC.Current = new StandardKernel();
            //IoC.Current.Bind<IActionInvoker>().To<ActionInvoker>();
            IoC.Current.Bind<IActionInvoker>().To<ViewRenderingActionInvoker>();
            IoC.Current.Bind<IIdentityService>().To<IdentityService>();
            IoC.Bind<IAppUrlPrefixes>().To<AppUrlPrefixes>();
            IoC.Bind<IBlogEntriesRepository>().To<BlogItemsRepository>().InSingletonScope();
            IoC.Bind<ITweetsRepository>().To<TweetsMockRepository>().InSingletonScope();

            //setup data repos
            IoC.Bind<IUsersRepository>().To<UsersThingRepository>();
            IoC.Bind<IPromptRepository>().To<PromptsThingRepository>();
            IoC.Bind<IPlansRepository>().To<PlansL2SRepository>();


            w.Stop();
            Console.WriteLine("Setup took " + w.Elapsed.TotalSeconds.ToString(NumberFormat.Number, 3) + " sec.");
            Console.WriteLine("Rendering Views");
            TempHtmlDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(TempHtmlDirectory);

            w.Restart();
            RenderViews();
            w.Stop();
            Console.WriteLine("Rendering views " + w.Elapsed.TotalSeconds.ToString(NumberFormat.Number, 3) + " sec.");
            
            Console.WriteLine("Copying Views");
            // robocopy "C:\Temp\MvcApplication2" "C:\Users\Steve\Dropbox\Takeoff Design\Latest Rips" *.html  /purge /xd .*
            string d = string.Format("\"{0}\" \"{1}\" /IS /E /XD .* /XF .*", TempHtmlDirectory, OutputPath);
            Process.Start(new ProcessStartInfo("robocopy")
            {
                Arguments = string.Format("\"{0}\" \"{1}\" /IS /E /XD .* /XF .*", TempHtmlDirectory, OutputPath)
            }).WaitForExit();

            if (options.FoldersToCopy.HasChars())
            {
                Console.WriteLine("Copying Asset Folders");//
                string[] foldersToCopy = options.FoldersToCopy.Split(new char[] { ',' }).Select(f => f.Trim()).Where(f => f.HasChars()).ToArray();
                foreach (var folder in foldersToCopy)
                {
                    string source = Path.Combine(AppDirectory, folder);
                    string outputDirectory = Path.Combine(OutputPath, folder);
                    Console.WriteLine("Copying " + folder);
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }
                    Process.Start(new ProcessStartInfo("robocopy")
                    {
                        Arguments = string.Format("\"{0}\" \"{1}\" /MIR /IS /E /S /XD .* /XF .*", source, outputDirectory)
                    }).WaitForExit();
                }
            }

            DeleteDirectory(TempHtmlDirectory);
        }

#region View Rendering

        private static void RenderViews()
        {
            RenderSharedViews();
            RenderViewsForRoot();
            RenderViewsForSignup();
            RenderViewsForAccount();
            RenderViewsForAccountPlan();
            RenderViewsForAccountMembers();
            RenderViewsForAccountMemberships();
            RenderViewsForAccountInvoices();
            RenderViewsForDashboard();
            RenderViewsForProductions();
            RenderViewsForMembershipRequests();
            RenderViewsForEmail();
            RenderViewsForErrors();
            RenderViewsForPrompts();
            RenderViewsForTrial();
        }

        #endregion

        public static bool DeleteDirectory(string directory)
        {
            bool result = false;

            string[] files = Directory.GetFiles(directory);
            string[] dirs = Directory.GetDirectories(directory);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(directory, false);

            return result;
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

    class IdentityService: IIdentityService
    {

        public Identity GetIdentity(HttpContextBase context)
        {
            return null;
        }

        public void SetIdentity(Identity value, IdentityPeristance peristance, HttpContextBase context)
        {
        }

        public void ClearIdentity(HttpContextBase context)
        {
        }
    }

    /// <summary>
    /// Used to fix links in ripped files.  
    /// </summary>
    class AppUrlPrefixes : IAppUrlPrefixes
    {

        public string Relative
        {
            get { return ""; }
        }

        public string RelativeHttp
        {
            get { return ""; }
        }

        public string RelativeHttps
        {
            get { return ""; }
        }

        public string AbsoluteHttp
        {
            get { return ""; }
        }

        public string AbsoluteHttps
        {
            get { return ""; }
        }

        public string Asset
        {
            get { return ""; }
        }

        public string Get(UrlType type)
        {
            return "";
        }

        public string FromRelative(string appRootUrl, UrlType type)
        {
            return appRootUrl;
        }

        public void Fill(HttpContextBase context, UrlHelper helper)
        {
        }
    }
}
