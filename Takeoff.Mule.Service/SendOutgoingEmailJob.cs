using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Takeoff.Jobs;
using System.Messaging;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using Takeoff.Models;

namespace Mule
{
    /// <summary>
    /// This job monitors an MSMQ message queue that has emails to send.  Then it sends them, honoring rate limiting.
    /// </summary>
    class SendOutgoingEmailJob : Job
    {
        public static int MillisecondsToWaitAfterSend = ConfigUtil.GetRequiredAppSetting<int>("OutgoingMailMillisecondsToWaitBetweenSends");

        public override object FetchTask()
        {
            var message = OutgoingMail.MessageQueue.Receive();
            if (message == null)
                return null;
            //message body is json
            var body = new StreamReader(message.BodyStream).ReadToEnd();
            var json = (from e in XDocument.Parse(body.ToString()).Descendants("string") select e.Value).FirstOrDefault();
            if ( json == null )
                return null;
            return JsonConvert.DeserializeObject<OutgoingMessage>(json);
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
            bool sentEmail = false;
            task.Step("Send", true, 3, TimeSpan.FromMinutes(.5), () =>
            {
                var message = fetchResult.CastTo<OutgoingMessage>();
                if (OutgoingMail.SendNow(message))
                {
                    sentEmail = true;
                    OutgoingMail.LogMail(message);
                }

            });
            task.Step("PauseForRateLimit", () =>
            {
                if (sentEmail)
                    Thread.Sleep(MillisecondsToWaitAfterSend);
            });
            return task;
        }
    }
}
