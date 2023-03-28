using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Messaging;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.ComponentModel;
using System.Threading;
using System.ServiceProcess;
using Takeoff.Jobs;
using Takeoff.Transcoder;

namespace Mule
{
    class Program
    {
        const string DeferredRequestHttpHeaderName = "x-deferredrequest";//so web app knows it's from deferred...otherwise certain action methods can't be executed. 

        static void Main(string[] args)
        {
            var argsTable = args.ArgsToTable();
            //startDelay is useful when attaching the debugger to a windows service or something
            if (argsTable.ContainsKey("startDelay"))
            {
                var seconds = argsTable["startDelay"].ToInt();
                Console.WriteLine("Starting job manager in " + seconds.ToInvariant() + " seconds.");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
            }

            var supervisor = new Supervisor();
            if (ConfigUtil.AppSettingOrDefault("EnablePerformDeferredWebRequestJob", true))
                supervisor.Jobs.Add(new PerformDeferredWebRequestJob());
            if (ConfigUtil.AppSettingOrDefault("EnablePerformDeferredMethodInvokeJob", true))
                supervisor.Jobs.Add(new PerformDeferredMethodInvokeJob());            
            if (ConfigUtil.AppSettingOrDefault("EnableSendOutgoingEmailJob", true))
                supervisor.Jobs.Add(new SendOutgoingEmailJob());
            if (ConfigUtil.AppSettingOrDefault("EnableEncodingStartedJobProcessor", true))
                supervisor.Jobs.Add(new EncodingStartedJobProcessor());
            if (ConfigUtil.AppSettingOrDefault("EnableEncodeJobWatcher", true))
                supervisor.Jobs.Add(new EncodeJobWatcher());
            if (ConfigUtil.AppSettingOrDefault("EnableCommandProcessorJob", true))
                supervisor.Jobs.Add(new CommandProcessorJob(supervisor, Environment.MachineName, "Mule"));

            if (argsTable.ContainsKey("console"))
            {
                supervisor.Start();
                Console.WriteLine("Started");
                Console.ReadLine();
            }
            else
            {
                ServiceBase.Run(new WindowsService(supervisor));
            }
        }


        /// <summary>
        /// Creates a JsonPoster that has the necessary url prefix and header for a mule http request.
        /// </summary>
        /// <returns></returns>
        public static JsonPoster CreateJsonPoster(string urlPrefix)
        {
            var poster = new JsonPoster(urlPrefix);
            poster.WebRequestCreated += (o, e) =>
            {
                var key = Guid.NewGuid().StripDashes();
                var hashKey = ConfigurationManager.AppSettings["DeferredRequestSecretKey"];
                poster.CurrentRequest.Headers.Add(DeferredRequestHttpHeaderName, key + ":" + key.Hash(hashKey, "HMACSHA1"));//so we know it's from the mule
                Console.Write("Requesting: " + poster.CurrentRequest.RequestUri.ToString());
            };
            return poster;
        }

    }

}
