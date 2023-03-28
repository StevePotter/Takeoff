using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Amazon.S3.Model;
using Amazon.SQS.Model;
using LitS3;
using Mediascend;
using Takeoff.Jobs;
using System.Collections;
using Takeoff.Transcoder.PandaStream;
using Newtonsoft.Json.Linq;
using Amazon.ElasticTranscoder;
using Amazon;

namespace Takeoff.Transcoder
{

    /// <summary>
    /// Creates and completes the process of transcoding a single source video into 1-x target encoding profiles using zencoder.
    /// </summary>
    public class EncodingTriageTask: JobTask<string>
    {
        public EncodingTriageTask(Message message)
        {
            Message = message;
            EnableXmlReport = true;
        }

        #region Properties

        /// <summary>
        /// All the info regarding the source video and target outputs.  Taken from the Message.
        /// </summary>
        public EncodeJobParams EncodingParams { get; private set; }

        /// <summary>
        /// The message taken from Amazon SQS.
        /// </summary>
        private Message Message { get; set; }

        /// <summary>
        /// The metadata for the source video.
        /// </summary>
        private VideoMetaData SourceMetaData { get; set; }

        /// <summary>
        /// The file path to the source video being transcoded.
        /// </summary>
        private string SourceVideoPath
        {
            get
            {
                if (EncodingParams == null)
                    return null;

                return Path.Combine(TempFolder, EncodingParams.InputOriginalFileName);
            }
        }

        /// <summary>
        /// The folder used to save sources while the task is running.
        /// </summary>
        string TempFolder;

        /// <summary>
        /// Amazon S3 service interface.
        /// </summary>
        /// <returns></returns>
        static S3Service S3Service
        {
            get
            {
                return new S3Service
                {
                    AccessKeyID = ConfigurationManager.AppSettings["AmazonAccessKey"],
                    SecretAccessKey = ConfigurationManager.AppSettings["AmazonSecretKey"]
                };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The primary function that kicks off the job.
        /// </summary>
        public void Execute()
        {
            Step("Job", () =>
            {
                Debug.Assert(Message != null);
                var totalTime = Stopwatch.StartNew();
                var jobElement = CurrentReportElement;
                jobElement.SetElementValue("MessageBody", Message.Body);
                jobElement.SetAttributeValue("MessageId", Message.MessageId);
                jobElement.SetAttributeValue("MachineId", Program.MachineId);
                jobElement.SetAttributeValue("StartTime", DateTime.Now.ToString(DateTimeFormat.ShortDateTime));

                Step("ParseMessage", () =>
                {
                    EncodingParams = new JavaScriptSerializer().Deserialize<EncodeJobParams>(Message.Body);
                    EncodingParams.EncodingTriageStarted = DateTime.UtcNow;
                    jobElement.SetAttributeValue("InputId", EncodingParams.InputId);
                });

                Step("CreateTempFolder", () =>
                {
                    TempFolder = Path.Combine(Path.Combine(Program.Directory, "TranscodeFiles"), EncodingParams.InputId.ToInvariant());
                    if (!Directory.Exists(TempFolder))
                    {
                        Directory.CreateDirectory(TempFolder);
                    }
                });

                Step("DownloadSource", false, 2, TimeSpan.FromSeconds(5), DownloadSourceVideo);
                Step("GetMetaData", GetMetaData);

                //make the request to zencoder api.  
                Step("RequestEncode", false, 5, TimeSpan.FromMinutes(2), RequestEncoding);
                Step("DeleteTempFiles", true, 1, TimeSpan.Zero, DeleteTempFiles);

                totalTime.Stop();
                jobElement.SetAttributeValue("EndTime", DateTime.Now.ToString(DateTimeFormat.ShortDateTime));
                jobElement.SetAttributeValue("TotalSeconds", (int)totalTime.Elapsed.TotalSeconds);

                Result = new JavaScriptSerializer().Serialize(EncodingParams);
            });

            Step("UploadReport", true, 3, TimeSpan.FromSeconds(4), false, () =>
            {
                if (EncodingParams != null)
                    Report.Root.SetAttributeValue("ErrorCode", EncodingParams.ErrorCode);
                S3Service.AddObjectString(Report.ToString(SaveOptions.None), ConfigurationManager.AppSettings["ReportBucket"], EncodingParams.InputId + ".xml", "application/xml", LitS3.CannedAcl.Private);
            });
        }

        /// <summary>
        /// Downloads the source video file.
        /// </summary>
        /// <returns></returns>
        /// <remarks>If this fails, it is often because the video was deleted by the user just after uploading.</remarks>
        private void DownloadSourceVideo()
        {
            AddReportAttribute("Url", EncodingParams.InputUrl);
            var webClient = new WebClient();
            webClient.DownloadFile(new Uri(EncodingParams.InputUrl), SourceVideoPath);
            if (!File.Exists(SourceVideoPath))
                throw new Exception("Source video couldn't be downloaded.");
            AddReportAttribute("LocalPath", SourceVideoPath);
            AddReportAttribute("FileSize", new FileSize((int)(new System.IO.FileInfo(SourceVideoPath).Length)));
            AddReportAttribute("FileSizeBytes", new System.IO.FileInfo(SourceVideoPath).Length);
        }

        /// <summary>
        /// Gets the source video metadata, setitng the MetaData property.
        /// </summary>
        private void GetMetaData()
        {
            this.SourceMetaData = VideoMetaData.Create(Program.Directory, SourceVideoPath);
            if (SourceMetaData == null)
                throw new Exception("Could not extract metadata from source video.");

            CurrentReportElement.Add(new XElement(SourceMetaData.RawMetaData.Root));

            CurrentReportElement.SetAttributeValue("IsVideo", SourceMetaData.HasVideo);

            if (!SourceMetaData.HasVideo)//will happen for mp3 uploads, txt, or whatever
            {
                EncodingParams.ErrorCode = TranscodeJobErrorCodes.NotAVideo;
                return;
            }

            CurrentReportElement.SetAttributeValue("Duration", SourceMetaData.Duration.GetValueOrDefault().TotalSeconds);
            CurrentReportElement.SetAttributeValue("VideoBitRate", SourceMetaData.VideoBitRate);
            CurrentReportElement.SetAttributeValue("AudioBitRate", SourceMetaData.AudioBitRate);
            CurrentReportElement.SetAttributeValue("FrameRate", SourceMetaData.FrameRate);
            CurrentReportElement.SetAttributeValue("Height", SourceMetaData.Height);
            CurrentReportElement.SetAttributeValue("Width", SourceMetaData.Width);


            SourceMetaData.GetGeneralAttribute("Format").IfNotNull(v => EncodingParams.InputFileFormat = v);
            SourceMetaData.GetAudioAttribute("Format").IfNotNull(v => EncodingParams.InputAudioFormat = v);
            SourceMetaData.GetVideoAttribute("Format").IfNotNull(v => EncodingParams.InputVideoFormat = v);
            EncodingParams.InputDuration = !SourceMetaData.Duration.HasValue ? 0.0 : SourceMetaData.Duration.Value.TotalSeconds;
            EncodingParams.InputAudioBitRate = SourceMetaData.AudioBitRate;
            EncodingParams.InputVideoBitRate = SourceMetaData.VideoBitRate;
            EncodingParams.InputFrameRate = SourceMetaData.FrameRate;
            EncodingParams.InputHeight = SourceMetaData.Height;
            EncodingParams.InputWidth = SourceMetaData.Width;

            //check this to prevent abuse
            if (EncodingParams.DurationLimit.HasValue &&  EncodingParams.DurationLimit <= EncodingParams.InputDuration)
            {
                EncodingParams.ErrorCode = TranscodeJobErrorCodes.DurationTooLong;
                return;
            }

            //compute keyframeinterval when KeyFramesPerSecond is set
            foreach (var output in EncodingParams.Outputs.Where(o=> o.KeyFramesPerSecond.IsPositive()))
            {
                if (SourceMetaData.FrameRate.IsPositive())
                {
                    output.KeyFrameInterval = (int)Math.Round(SourceMetaData.FrameRate / (double)output.KeyFramesPerSecond);
                }

                //when -g is 1, it doesn't work, and only 1 keyframe in the way beginnign of the video is inserted.  couldn't find why in docs, but i prevent that here.  
                //todo: test 1 with zencoder
                if (output.KeyFrameInterval <= 1)
                {
                    output.KeyFrameInterval = 2;
                }
            }
        }


        /// <summary>
        /// This is where you can choose the proper encoding platform and then make the request to encode.  For now it's just for Zencoder.
        /// </summary>
        private void RequestEncoding()
        {
            EncodingParams.Encoder = EncodingParams.Encoder.CharsOr("Zencoder");
            EncodingParams.EncodingRequested = DateTime.UtcNow;
            AddReportAttribute("Encoder", EncodingParams.Encoder);

            var encoder = EncodingParams.Encoder.CharsOr("zencoder");
            if (encoder.EqualsCaseInsensitive("zencoder"))
                RequestFromZencoder();
            else if (encoder.EqualsCaseInsensitive("amazon"))
                RequestFromAmazon();
            else
                RequestFromPanda();
        }

        private void RequestFromZencoder()
        {
            var thumbLocation = new S3FileLocation
                                    {
                                        Location = EncodingParams.ThumbnailLocation
                                    };

            var prefix = Guid.NewGuid().StripDashes();
            var zencoderRequest = new ZencoderCreateJobRequest()
                                      {
                                          ApiKey = ConfigurationManager.AppSettings["ZencoderApiKey"],
                                          Input = EncodingParams.InputUrl,
                                          Test = EncodingParams.Test,
                                          ThumbnailAcl = EncodingParams.ThumbnailAcl,
                                          ThumbnailBaseUrl = "s3://" + thumbLocation.Bucket + "/" + thumbLocation.FolderInBucket,
                                          ThumbnailCount = EncodingParams.ThumbnailCount,
                                          ThumbnailMaxHeight = EncodingParams.ThumbnailMaxHeight,
                                          ThumbnailMaxWidth = EncodingParams.ThumbnailMaxWidth,
                                          ThumbnailPrefix = prefix,
                                          Region = "us",
                                      };

            List<ZencoderOutputParams> zencoderOutputs = new List<ZencoderOutputParams>();
            foreach (var outputRequest in EncodingParams.Outputs)
            {
                var zencoderOutput = new ZencoderOutputParams();
                zencoderOutputs.Add(zencoderOutput);
                zencoderOutput.TargetUrl = outputRequest.Url;
                zencoderOutput.Label = outputRequest.Profile;
                if (outputRequest.KeyFrameInterval > 0)
                    zencoderOutput.KeyFrameInterval = outputRequest.KeyFrameInterval;
                outputRequest.AudioSampleRate.IfHasVal(v => zencoderOutput.AudioSampleRate = v);
                outputRequest.Width.IfHasVal(v => zencoderOutput.Width = v);
                outputRequest.Height.IfHasVal(v => zencoderOutput.Height = v);
                outputRequest.TargetAudioBitrate.IfHasVal(v => zencoderOutput.AudioBitrate = v);
                outputRequest.TargetAudioQuality.IfHasVal(v => zencoderOutput.AudioQuality = v);
                outputRequest.AudioSampleRate.IfHasVal(v => zencoderOutput.AudioSampleRate = v);
                outputRequest.TargetVideoBitrate.IfHasVal(v => zencoderOutput.VideoBitrate = v);
                outputRequest.TargetMaxVideoBitrate.IfHasVal(v => zencoderOutput.MaxBitrate = v);
                outputRequest.TargetVideoQuality.IfHasVal(v => zencoderOutput.VideoQuality = v);
                outputRequest.MaxFrameRate.IfHasVal(v => zencoderOutput.MaxFramerate = (int) v);
            }
            zencoderRequest.Outputs = zencoderOutputs.ToArray();

            var client = new ZencoderClient();
            var apiResponse = client.CreateJob(zencoderRequest);
            if (apiResponse == null)
                throw new Exception("Empty api response.");

            EncodingParams.EncoderJobId = Convert.ToString(apiResponse["id"]);
            AddReportAttribute("ZencoderResponse", new JavaScriptSerializer().Serialize(apiResponse));
        }

        private void RequestFromPanda()
        {
            PandaClient client = new PandaClient(ConfigurationManager.AppSettings["PandaCloudId"], ConfigurationManager.AppSettings["PandaApiKey"], ConfigurationManager.AppSettings["PandaSecretKey"], ConfigurationManager.AppSettings["PandaApiUrl"]);

            var thumbLocation = new S3FileLocation
            {
                Location = EncodingParams.ThumbnailLocation
            };

            var prefix = Guid.NewGuid().StripDashes();
            List<string> profileIds = new List<string>();
            profileIds.Add("thumbnails-5");
            foreach (var outputRequest in EncodingParams.Outputs)
            {
                var props = new Dictionary<string, string>();
                if (outputRequest.KeyFrameInterval > 0)
                    props["keyframe_interval"] = outputRequest.KeyFrameInterval.ToInvariant();
                outputRequest.Width.IfHasVal(v => props["width"] = v.ToInvariant());
//                outputRequest.Height.IfHasVal(v => props["height"] = v.ToInvariant());
                outputRequest.TargetAudioBitrate.IfHasVal(v => props["audio_bitrate"] = v.ToInvariant());
//                outputRequest.TargetAudioQuality.IfHasVal(v => zencoderOutput.AudioQuality = v);
                outputRequest.AudioSampleRate.IfHasVal(v => props["audio_sample_rate"] = v.ToInvariant());
                props["title"] = outputRequest.Profile;
                props["name"] = outputRequest.Profile + "-" + EncodingParams.InputId.ToInvariant();
                if (outputRequest.Profile.EqualsCaseInsensitive("Mobile"))
                {
                    props["preset_name"] = "h264";
                }
                else
                {
                    props["preset_name"] = "h264.hi";
                }
                outputRequest.TargetVideoBitrate.IfHasVal(v => props["video_bitrate"] = v.ToInvariant());
//                outputRequest.TargetMaxVideoBitrate.IfHasVal(v => props["height"] = v.ToInvariant());
//                outputRequest.TargetVideoQuality.IfHasVal(v => props["height"] = v.ToInvariant());
//                outputRequest.MaxFrameRate.IfHasVal(v => zencoderOutput.MaxFramerate = (int)v);

//                var hash = props.ToArray().OrderBy(pair => pair.Key).Select(pair => pair.Key + ":" + pair.Value).Join(",").MD5Hash();
                
                var profile = client.PostJson("profiles.json", props);
                dynamic json = JObject.Parse(profile);
                string id = json["id"].Value;
                profileIds.Add(id);
            }

                              //ApiKey = ConfigurationManager.AppSettings["ZencoderApiKey"],
                              //            Input = EncodingParams.InputUrl,
                              //            Test = EncodingParams.Test,
                              //            ThumbnailAcl = EncodingParams.ThumbnailAcl,
                              //            ThumbnailBaseUrl = "s3://" + thumbLocation.Bucket + "/" + thumbLocation.FolderInBucket,
                              //            ThumbnailCount = EncodingParams.ThumbnailCount,
                              //            ThumbnailMaxHeight = EncodingParams.ThumbnailMaxHeight,
                              //            ThumbnailMaxWidth = EncodingParams.ThumbnailMaxWidth,
                              //            ThumbnailPrefix = prefix,
                              //            Region = "us",

            var postResult = client.PostJson("videos.json", new Dictionary<string, string>{
                {"source_url", EncodingParams.InputUrl},
                {"profiles",profileIds.ToArray().Join(",")}
            });
            dynamic videoJson = JObject.Parse(postResult);
            string videoId = videoJson["id"].Value;
            EncodingParams.EncoderJobId = videoId;
            AddReportAttribute("PandaResponse", new JavaScriptSerializer().Serialize(postResult));
        }

        private void RequestFromAmazon()
        {
            AmazonElasticTranscoderClient client = new AmazonElasticTranscoderClient(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]);

            string pipeline = ConfigurationManager.AppSettings["AmazonTranscodingPipeline"];
            client.ReadPipeline(new Amazon.ElasticTranscoder.Model.ReadPipelineRequest { Id = "video" });

            var thumbLocation = new S3FileLocation
            {
                Location = EncodingParams.ThumbnailLocation
            };

            client.ReadPreset(new Amazon.ElasticTranscoder.Model.ReadPresetRequest
            {
                                      Id = "asdf"
                                  });

            var preset = new Amazon.ElasticTranscoder.Model.Preset
                             {
                                 Thumbnails = new Amazon.ElasticTranscoder.Model.Thumbnails
                                                  {

                                                  },
                                 Video = new Amazon.ElasticTranscoder.Model.VideoParameters
                                             {
                                                 
                                             }

                             };
            var prefix = Guid.NewGuid().StripDashes();
            List<string> profileIds = new List<string>();
            profileIds.Add("thumbnails-5");
//ThumbnailCount = 5,
//                ThumbnailMaxHeight = 150,
//                ThumbnailMaxWidth = 180,

                    //new EncodeJobOutputVideo
                    //{
                    //    Profile = "Web",
                    //    Url = "s3://" + outputVideo.Bucket + "/" + outputVideo.Key + "-w.mp4",
                    //    KeyFramesPerSecond = 5,
                    //    TargetAudioQuality = 3,
                    //    TargetVideoQuality = 2,
                    //    MaxFrameRate = 30,
                    //    Width = 960//the max width of the video.  zencoder charges double for 1280x720, plus we don't go full screen so there's no point really.  
                    //},
                    //new EncodeJobOutputVideo
                    //{
                    //    Profile = "Mobile",
                    //    Url = "s3://" + outputVideo.Bucket + "/" + outputVideo.Key + "-m.mp4",
                    //    KeyFramesPerSecond = 5,
                    //    TargetAudioBitrate = 96,
                    //    AudioSampleRate = 44100,                        
                    //    TargetVideoBitrate = 900,
                    //    MaxFrameRate = 30,
                    //    Width = 480,
                    //    Height = 320
                    //},

            //foreach (var outputRequest in EncodingParams.Outputs)
            //{
            //    var props = new Dictionary<string, string>();
            //    if (outputRequest.KeyFrameInterval > 0)
            //        props["keyframe_interval"] = outputRequest.KeyFrameInterval.ToInvariant();
            //    outputRequest.Width.IfHasVal(v => props["width"] = v.ToInvariant());
            //    //                outputRequest.Height.IfHasVal(v => props["height"] = v.ToInvariant());
            //    outputRequest.TargetAudioBitrate.IfHasVal(v => props["audio_bitrate"] = v.ToInvariant());
            //    //                outputRequest.TargetAudioQuality.IfHasVal(v => zencoderOutput.AudioQuality = v);
            //    outputRequest.AudioSampleRate.IfHasVal(v => props["audio_sample_rate"] = v.ToInvariant());
            //    props["title"] = outputRequest.Profile;
            //    props["name"] = outputRequest.Profile + "-" + EncodingParams.InputId.ToInvariant();
            //    if (outputRequest.Profile.EqualsCaseInsensitive("Mobile"))
            //    {
            //        props["preset_name"] = "h264";
            //    }
            //    else
            //    {
            //        props["preset_name"] = "h264.hi";
            //    }
            //    outputRequest.TargetVideoBitrate.IfHasVal(v => props["video_bitrate"] = v.ToInvariant());
            //    //                outputRequest.TargetMaxVideoBitrate.IfHasVal(v => props["height"] = v.ToInvariant());
            //    //                outputRequest.TargetVideoQuality.IfHasVal(v => props["height"] = v.ToInvariant());
            //    //                outputRequest.MaxFrameRate.IfHasVal(v => zencoderOutput.MaxFramerate = (int)v);

            //    //                var hash = props.ToArray().OrderBy(pair => pair.Key).Select(pair => pair.Key + ":" + pair.Value).Join(",").MD5Hash();

            //    var profile = client.PostJson("profiles.json", props);
            //    dynamic json = JObject.Parse(profile);
            //    string id = json["id"].Value;
            //    profileIds.Add(id);
            //}

            ////ApiKey = ConfigurationManager.AppSettings["ZencoderApiKey"],
            ////            Input = EncodingParams.InputUrl,
            ////            Test = EncodingParams.Test,
            ////            ThumbnailAcl = EncodingParams.ThumbnailAcl,
            ////            ThumbnailBaseUrl = "s3://" + thumbLocation.Bucket + "/" + thumbLocation.FolderInBucket,
            ////            ThumbnailCount = EncodingParams.ThumbnailCount,
            ////            ThumbnailMaxHeight = EncodingParams.ThumbnailMaxHeight,
            ////            ThumbnailMaxWidth = EncodingParams.ThumbnailMaxWidth,
            ////            ThumbnailPrefix = prefix,
            ////            Region = "us",

            //var postResult = client.PostJson("videos.json", new Dictionary<string, string>{
            //    {"source_url", EncodingParams.InputUrl},
            //    {"profiles",profileIds.ToArray().Join(",")}
            //});
            //dynamic videoJson = JObject.Parse(postResult);
            //string videoId = videoJson["id"].Value;
            //EncodingParams.EncoderJobId = videoId;
            //AddReportAttribute("PandaResponse", new JavaScriptSerializer().Serialize(postResult));

            //client.Dispose();
        }


        public override bool ErrorOccured
        {
            get
            {
                return base.ErrorOccured || (EncodingParams != null && EncodingParams.ErrorCode != TranscodeJobErrorCodes.NotSet);
            }
        }

        protected override void OnErrorOccured(Exception ex)
        {
            base.OnErrorOccured(ex);
            if (EncodingParams != null && EncodingParams.ErrorCode == TranscodeJobErrorCodes.NotSet)
                EncodingParams.ErrorCode = TranscodeJobErrorCodes.Other;
        }

        /// <summary>
        /// Deletes the temp files directory.
        /// </summary>
        private void DeleteTempFiles()
        {
            //exceptions could be raised if a file is in use somehow.  shouldn't happen but we don't want that to kill the job...so wrap the delete calls in empty try/catch
            foreach (var file in Directory.GetFiles(this.TempFolder))
            {
                try
                {
                    File.Delete(file);
                    WriteLine("Deleted " + file);
                }
                catch
                {
                }
            }

            try
            {
                Directory.Delete(TempFolder);
                WriteLine("Deleted " + TempFolder);
            }
            catch
            {
            }
        }


        #endregion
    }

}
