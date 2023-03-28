//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Diagnostics;
//using System.Drawing;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading;
//using System.Web.Script.Serialization;
//using System.Xml.Linq;
//using Amazon.S3.Model;
//using Amazon.SQS.Model;
//using LitS3;
//using Mediascend;
//using Takeoff.Jobs;
//namespace Takeoff.Transcoder
//{

//    /// <summary>
//    /// Creates and completes the process of transcoding a single source video into 1-x target encoding profiles using zencoder.
//    /// </summary>
//    public class ZencoderTask: JobTask<string>
//    {
//        public ZencoderTask(Message message)
//        {
//            Message = message;
//            EnableXmlReport = true;
//        }

//        #region Properties

//        /// <summary>
//        /// All the info regarding the source video and target outputs.  Taken from the Message.
//        /// </summary>
//        public ZencoderCreateJobRequest Request { get; private set; }


//        public ZencoderCreateJobResponse JobResult { get; private set; }

//        /// <summary>
//        /// The message taken from Amazon SQS.
//        /// </summary>
//        private Message Message { get; set; }

//        /// <summary>
//        /// The metadata for the source video.
//        /// </summary>
//        private VideoMetaData SourceMetaData { get; set; }

//        /// <summary>
//        /// The file path to the source video being transcoded.
//        /// </summary>
//        private string SourceVideoPath
//        {
//            get
//            {
//                if (Request == null)
//                    return null;

//                return Path.Combine(TempFolder, Request.InputFileName);
//            }
//        }

//        /// <summary>
//        /// The folder used to save sources while the task is running.
//        /// </summary>
//        string TempFolder;

//        /// <summary>
//        /// Amazon S3 service interface.
//        /// </summary>
//        /// <returns></returns>
//        static S3Service S3Service
//        {
//            get
//            {
//                return new S3Service
//                {
//                    AccessKeyID = ConfigurationManager.AppSettings["AmazonAccessKey"],
//                    SecretAccessKey = ConfigurationManager.AppSettings["AmazonSecretKey"]
//                };
//            }
//        }

//        #endregion

//        #region Methods

//        /// <summary>
//        /// The primary function that kicks off the job.
//        /// </summary>
//        public void Execute()
//        {
//            Step("Job", () =>
//            {
//                Debug.Assert(Message != null);
//                var totalTime = Stopwatch.StartNew();
//                var jobElement = CurrentReportElement;
//                jobElement.SetElementValue("MessageBody", Message.Body);
//                jobElement.SetAttributeValue("MessageId", Message.MessageId);
//                jobElement.SetAttributeValue("MachineId", Program.MachineId);
//                jobElement.SetAttributeValue("StartTime", DateTime.Now.ToFormattedString(DateTimeValueFormat.ShortDateTime));

//                JobResult = new ZencoderCreateJobResponse();
//                Step("ParseMessage", () =>
//                {
//                    Request = new JavaScriptSerializer().Deserialize<ZencoderCreateJobRequest>(Message.Body);
//                    jobElement.SetAttributeValue("TakeoffJobId", Request.TakeoffJobId);
//                    JobResult.TakeoffJobId = Request.TakeoffJobId;
//                });

//                Step("CreateTempFolder", () =>
//                {
//                    TempFolder = Path.Combine(Path.Combine(Program.Directory, "TranscodeFiles"), Request.TakeoffJobId);
//                    if (!Directory.Exists(TempFolder))
//                    {
//                        Directory.CreateDirectory(TempFolder);
//                    }
//                });

//                Step("DownloadSource", false, 2, TimeSpan.FromSeconds(5), DownloadSourceVideo);
//                Step("GetMetaData", GetMetaData);

//                //make the request to zencoder api.  
//                Step("ZencoderRequest", false, 10, TimeSpan.FromMinutes(2), () =>
//                {
//                    var client = new ZencoderClient();
//                    var apiResponse = client.CreateJob(Request);
//                    if ( apiResponse == null )
//                        throw new Exception("Empty api response.");

//                    JobResult.ZencoderJobId = (int)apiResponse["id"];
//                    JobResult.ZencoderResponse = apiResponse;
//                    AddReportAttribute("ZencoderResponse", new JavaScriptSerializer().Serialize(apiResponse)); 
//                });

//                Step("DeleteTempFiles", true, 1, TimeSpan.Zero, DeleteTempFiles);

//                totalTime.Stop();
//                JobResult.JobDuration = (int)totalTime.Elapsed.TotalSeconds;
//                jobElement.SetAttributeValue("EndTime", DateTime.Now.ToFormattedString(DateTimeValueFormat.ShortDateTime));

//                Result = new JavaScriptSerializer().Serialize(JobResult);
//            });

//            Step("UploadReport", true, 3, TimeSpan.FromSeconds(4), false, () =>
//            {
//                if (JobResult != null)
//                    Report.Root.SetAttributeValue("ErrorCode", JobResult.ErrorCode);
//                S3Service.AddObjectString(Report.ToString(SaveOptions.None), ConfigurationManager.AppSettings["ReportBucket"], Request.TakeoffJobId + ".xml", "application/xml", LitS3.CannedAcl.Private);
//            });
//        }

//        /// <summary>
//        /// Downloads the source video file.
//        /// </summary>
//        /// <returns></returns>
//        /// <remarks>If this fails, it is often because the video was deleted by the user just after uploading.</remarks>
//        private void DownloadSourceVideo()
//        {
//            AddReportAttribute("Url", Request.Input);
//            AddReportAttribute("FileName", Request.InputFileName);
//            var webClient = new WebClient();
//            webClient.DownloadFile(new Uri(Request.Input), SourceVideoPath);
//            if (!File.Exists(SourceVideoPath))
//                throw new Exception("Source video couldn't be downloaded.");
//            AddReportAttribute("LocalPath", SourceVideoPath);
//            AddReportAttribute("FileSize", FileUtil.FormatFileSize((int)(new System.IO.FileInfo(SourceVideoPath).Length)));
//            AddReportAttribute("FileSizeBytes", new System.IO.FileInfo(SourceVideoPath).Length);
//        }

//        /// <summary>
//        /// Gets the source video metadata, setitng the MetaData property.
//        /// </summary>
//        private void GetMetaData()
//        {
//            this.SourceMetaData = VideoMetaData.Create(Program.Directory, SourceVideoPath);
//            if (SourceMetaData == null)
//                throw new Exception("Could not extract metadata from source video.");

//            CurrentReportElement.Add(new XElement(SourceMetaData.RawMetaData.Root));

//            CurrentReportElement.SetAttributeValue("IsVideo", SourceMetaData.HasVideo);
//            CurrentReportElement.SetAttributeValue("Duration", SourceMetaData.Duration.GetValueOrDefault().TotalSeconds);
//            CurrentReportElement.SetAttributeValue("VideoBitRate", SourceMetaData.VideoBitRate);
//            CurrentReportElement.SetAttributeValue("AudioBitRate", SourceMetaData.AudioBitRate);
//            CurrentReportElement.SetAttributeValue("FrameRate", SourceMetaData.FrameRate);
//            CurrentReportElement.SetAttributeValue("Height", SourceMetaData.Height);
//            CurrentReportElement.SetAttributeValue("Width", SourceMetaData.Width);

//            if (!SourceMetaData.HasVideo)//will happen for mp3 uploads, txt, or whatever
//            {
//                JobResult.ErrorCode = TranscodeJobErrorCodes.NotAVideo;
//                return;
//            }

//            JobResult.SourceDuration = !SourceMetaData.Duration.HasValue ? 0.0 : SourceMetaData.Duration.Value.TotalSeconds;

//            //compute keyframeinterval when KeyFramesPerSecond is set
//            foreach (var output in Request.Outputs.Where(o=> o.KeyFramesPerSecond.GetValueOrDefault() > 0))
//            {
//                if (SourceMetaData.FrameRate.IsPositive())
//                {
//                    output.KeyFrameInterval = (int)Math.Round(SourceMetaData.FrameRate / (double)output.KeyFramesPerSecond);
//                }

//                //when -g is 1, it doesn't work, and only 1 keyframe in the way beginnign of the video is inserted.  couldn't find why in docs, but i prevent that here.  
//                //todo: test 1 with zencoder
//                if (output.KeyFrameInterval <= 1)
//                {
//                    output.KeyFrameInterval = 2;
//                }
//            }
//        }


//        public override bool ErrorOccured
//        {
//            get
//            {
//                return base.ErrorOccured || (JobResult != null && JobResult.ErrorCode != TranscodeJobErrorCodes.NotSet);
//            }
//        }

//        protected override void OnErrorOccured(Exception ex)
//        {
//            base.OnErrorOccured(ex);
//            if (JobResult != null && JobResult.ErrorCode == TranscodeJobErrorCodes.NotSet)
//                JobResult.ErrorCode = TranscodeJobErrorCodes.Other;
//        }

//        /// <summary>
//        /// Deletes the temp files directory.
//        /// </summary>
//        private void DeleteTempFiles()
//        {
//            //exceptions could be raised if a file is in use somehow.  shouldn't happen but we don't want that to kill the job...so wrap the delete calls in empty try/catch
//            foreach (var file in Directory.GetFiles(this.TempFolder))
//            {
//                try
//                {
//                    File.Delete(file);
//                    WriteLine("Deleted " + file);
//                }
//                catch
//                {
//                }
//            }

//            try
//            {
//                Directory.Delete(TempFolder);
//                WriteLine("Deleted " + TempFolder);
//            }
//            catch
//            {
//            }
//        }


//        #endregion
//    }

//}
