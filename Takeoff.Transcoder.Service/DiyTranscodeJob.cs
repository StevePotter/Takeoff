//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Amazon.SQS;
//using System.Configuration;
//using Amazon.SQS.Model;
//using Takeoff.Jobs;
//using System.Threading;

//namespace Takeoff.Transcoder
//{
//    /// <summary>
//    /// A "do it yourself" transcode job where we use ffmpeg ourselves to do the work.
//    /// </summary>
//    class DiyTranscodeJob: SqsJob
//    {
//        public DiyTranscodeJob()
//        {
//            this.SqsVisibilityTimeout = TimeSpan.FromHours(4);
//            this.SleepTimeAfterNoMessageRecieved = TimeSpan.FromSeconds(10);
//        }

//        #region Properties 

//        /// <summary>
//        /// The queue name that recieves transcode requests.
//        /// </summary>
//        protected override string IncomingQueueName
//        {
//            get {
//                return ConfigurationManager.AppSettings["DiyTranscodeRequestQueue"];
//            }
//        }

//        protected override string OutgoingQueueName
//        {
//            get
//            {
//                return ConfigurationManager.AppSettings["DiyTranscodeResultQueue"];
//            }
//        }

//        #endregion

//        #region Methods

//        public override JobTask<string> PerformTask(Message fetchResult)
//        {
//            var task = new DiyTranscodeTask(fetchResult);
//            task.Execute();
//            return task;
//        }

//        #endregion

//    }
//}
