using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Jobs;
using System.Messaging;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using Takeoff.Transcoder;

namespace Mule
{
    /// <summary>
    /// This job monitors an MSMQ message queue that provides urls to hit.  This allows for offloading of work like sending emails, which results in a faster web app.
    /// </summary>
    class PerformDeferredWebRequestJob: Job
    {
        const string DeferredRequestHttpHeaderName = "x-deferredrequest";//so web app knows it's from deferred...otherwise certain action methods can't be executed. 

        public static MessageQueue MessageQueue
        {
            get
            {
                if (_MessageQueue == null)
                    _MessageQueue = new MessageQueue(ConfigUtil.GetRequiredAppSetting("DeferredWebRequestQueue"));
                return _MessageQueue;
            }
        }
        private static MessageQueue _MessageQueue;

        public override object FetchTask()
        {
            var message = MessageQueue.Receive();
            //message body is xml with a bunch of <string> elements each containing a url to execute
            var body = new StreamReader(message.BodyStream).ReadToEnd();
            var urls = (from e in XDocument.Parse(body.ToString()).Descendants("string") select e.Value).ToArray();
            if (urls != null && urls.Length > 0)
                return urls;
            else
                return null;
        }

        protected override void OnFetchException(Exception ex)
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        protected override void OnTaskException(Exception ex)
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        public override JobTask PerformTask(object fetchResult)
        {
            var task = new JobTask();
            var urlsToExecute = (string[])fetchResult;
            if (urlsToExecute != null && urlsToExecute.Length > 0)
            {
                foreach (var url in urlsToExecute)
                {
                    task.Step("Request", true, 3, TimeSpan.FromMinutes(.5), () =>
                    {
                        WriteLine("Executing " + url);
                        Program.CreateJsonPoster(null).PostJson(url);
                        WriteLine("Executed " + url);
                    });
                }
            }
            return task;
        }

    }
}
