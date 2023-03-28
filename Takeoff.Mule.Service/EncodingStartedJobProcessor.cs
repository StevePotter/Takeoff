using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Jobs;
using System.Messaging;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using Amazon.SQS;
using System.Web.Script.Serialization;
using System.Web;
using Takeoff.Transcoder;


namespace Mule
{
    /// <summary>
    /// Recieves the results of a newly created encoded job that, unless an error occured like bad input, it will already be in the zencoder system.  This will update the database with the zencoder job ID.  The job won't necessarily be complete yet.
    /// </summary>
    class EncodingStartedJobProcessor : SqsJob
    {
        public EncodingStartedJobProcessor()
        {
            this.SqsVisibilityTimeout = TimeSpan.FromMinutes(2);
            this.SleepTimeAfterNoMessageRecieved = TimeSpan.FromSeconds(10);
        }

        #region Properties

        protected override string IncomingQueueName
        {
            get { return ConfigurationManager.AppSettings["EncodingTriageResultQueue"]; }
        }

        /// <summary>
        /// The url to the API call that lets the web app know whether the job was created or an error occured (not a video or whatever).  
        /// </summary>
        private const string JobCreateResultNotificationUrl = "/Videos/UpdateEncodedJob";

        #endregion

        #region Methods

        public override JobTask<string> PerformTask(Amazon.SQS.Model.Message fetchResult)
        {
            var task = new JobTask<string>();
            EncodeJobParams encodeJobParams = null;
            
            task.Step("ParseMessage", () =>
            {
                encodeJobParams = new JavaScriptSerializer().Deserialize<EncodeJobParams>(fetchResult.Body);
            });

            //now we update the database by making a call to the API.  
            task.Step("AppRequest", false, 5, TimeSpan.FromSeconds(10), () =>
            {
                Program.CreateJsonPoster(ConfigurationManager.AppSettings["UrlPrefix"]).PostJson(JobCreateResultNotificationUrl, new
                {
                    jobParams = encodeJobParams,
                });
            });

            return task;
        }



        #endregion

    }
}
