using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Takeoff;
using Takeoff.Jobs;
using System.Messaging;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace Mule
{
    /// <summary>
    /// This job monitors an MSMQ message queue that provides deferred methods to invoke.
    /// </summary>
    class PerformDeferredMethodInvokeJob : Job
    {

        public static MessageQueue MessageQueue
        {
            get
            {
                if (_MessageQueue == null)
                    _MessageQueue = new MessageQueue(ConfigUtil.GetRequiredAppSetting("DeferredInvokesQueue"));
                return _MessageQueue;
            }
        }
        private static MessageQueue _MessageQueue;


        public override object FetchTask()
        {
            var message = MessageQueue.Receive();
            //message body is xml with a bunch of <string> elements each containing a url to execute
            var body = new StreamReader(message.BodyStream).ReadToEnd();
            var invokes = (from e in XDocument.Parse(body.ToString()).Descendants("string") select e.Value).Select(json => JsonConvert.DeserializeObject<DeferredMethodInvokeInfo>(json)).ToArray();
            if (invokes != null && invokes.Length > 0)
                return invokes;
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
            var deferredMethodInvokeInfos = (DeferredMethodInvokeInfo[])fetchResult;
            if (deferredMethodInvokeInfos != null && deferredMethodInvokeInfos.Length > 0)
            {
                foreach (var deferredMethodInvokeInfo in deferredMethodInvokeInfos)
                {
                    task.Step("Invoke", true, 3, TimeSpan.FromMinutes(.5), () =>
                    {
                        DeferredMethodInvoking.InvokeFromSerializedInvokeInfo(deferredMethodInvokeInfo);
                    });
                }
            }
            return task;
        }
    }
}
