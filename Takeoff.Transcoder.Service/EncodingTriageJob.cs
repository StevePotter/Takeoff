using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.SQS;
using System.Configuration;
using Amazon.SQS.Model;
using Takeoff.Jobs;
using System.Threading;

namespace Takeoff.Transcoder
{
    /// <summary>
    /// A transcode job that uses the zencoder.com platform.
    /// </summary>
    class EncodingTriageJob: SqsJob
    {
        public EncodingTriageJob()
        {
            this.SqsVisibilityTimeout = TimeSpan.FromMinutes(30);//should never take longer than that
            this.SleepTimeAfterNoMessageRecieved = TimeSpan.FromSeconds(6);
        }

        #region Properties 

        protected override string IncomingQueueName
        {
            get {
                return ConfigurationManager.AppSettings["EncodingTriageRequestQueue"];
            }
        }

        protected override string OutgoingQueueName
        {
            get
            {
                return ConfigurationManager.AppSettings["EncodingTriageResultQueue"];
            }
        }

        #endregion

        #region Methods

        public override JobTask<string> PerformTask(Message fetchResult)
        {
            var task = new EncodingTriageTask(fetchResult);
            task.Execute();
            return task;
        }

        #endregion

    }
}
