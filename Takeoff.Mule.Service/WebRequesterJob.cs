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

namespace Mule
{
    /// <summary>
    /// This job monitors an MSMQ message queue that provides urls to hit.  This allows for offloading of work like sending emails, which results in a faster web app.
    /// </summary>
    class WebRequesterJob: Job
    {

        private MessageQueue Queue
        {
            get
            {
                if ( _queue == null )
                {
                    var queueName = ConfigurationManager.AppSettings["MuleMessageQueue"];
                    Args.HasChars(queueName, "queueName");

                    if (!MessageQueue.Exists(queueName))
                        MessageQueue.Create(queueName, false);//i went for non-transactional because the chances of losing messages are super slim and nontransactional is faster (faster web request service). plus app should be able to recover from lost messages

                    //Connect to the queue
                    _queue = new MessageQueue(queueName);
                }
                return _queue;
            }
        }
        private MessageQueue _queue;

        public override object FetchTask()
        {
            var message = Queue.Receive();
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
                        Program.MuleJsonPost(url);
                        WriteLine("Executed " + url);
                    });
                }
            }
            return task;
        }
    }
}
