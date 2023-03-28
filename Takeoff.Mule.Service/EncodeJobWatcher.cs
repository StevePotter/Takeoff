using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Takeoff.Jobs;
using System.Messaging;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using Amazon.SQS;
using System.Web.Script.Serialization;
using System.Web;
using Takeoff.Models;
using Takeoff.Transcoder;
using System.Threading;
using System.Collections;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using Takeoff;
using Takeoff.Transcoder.PandaStream;
using Newtonsoft.Json.Linq;


namespace Mule
{
    /// <summary>
    /// Gets a list of jobs from the web app that are currently marked as in the zencoder system.  Checks them against zencoder api and updates the data accordingly. 
    /// </summary>
    class EncodeJobWatcher : Job
    {
        public EncodeJobWatcher()
        {
        }

        #region Properties

        /// <summary>
        /// Updates a particular video that has finished encoding and/or had an error.
        /// </summary>
        private const string ProcessEncodeResultUrl = "/Videos/ProcessEncodeResult";

        /// <summary>
        /// The url format string to the GET api that will return a job's status.
        /// </summary>
        private static string ZencoderJobStatusUrl = "https://app.zencoder.com/api/jobs/{0}?api_key={1}";

        private static string ZencoderApiKey = ConfigurationManager.AppSettings["ZencoderApiKey"];

        #endregion

        #region Methods

        public override object FetchTask()
        {
            var videos = Videos.VideosBeingEncoded();
            if (videos == null || videos.Length == 0)
                return null;
            return videos;
        }

        protected override void OnFetchUnsuccessful()
        {
            Thread.Sleep(TimeSpan.FromSeconds(20));
        }

        public override JobTask PerformTask(object fetchResult)
        {
            var wait = false;
            var task = new JobTask();
            foreach (var jobParams in (EncodeJobParams[])fetchResult)
            {
                if (jobParams.Encoder.EqualsCaseInsensitive("Zencoder"))
                {
                    if (ProcessPendingZencoderEncode(task, jobParams))
                        wait = true;
                }

            }

            if (wait)
                Thread.Sleep(TimeSpan.FromSeconds(5));
            return task;
        }

        private static bool ProcessPendingZencoderEncode(JobTask task, EncodeJobParams jobParams)
        {
            bool wait = false;
            EncodeJobParams @params = jobParams;
            task.Step("Processing Job", () =>
            {
                var inputLocation = S3FileLocation.FromUrl(@params.InputUrl);
                Dictionary<string, object> zencoderJobResults = null;
                string state = null;
                bool postResults = false;

                task.Step("Parsing Job Results", () =>
                {
                    var jobStatusUrl = string.Format(ZencoderJobStatusUrl, @params.EncoderJobId, ZencoderApiKey);
                    zencoderJobResults = new JsonPoster().GetJson<Dictionary<string, object>>(jobStatusUrl);
                    zencoderJobResults = (Dictionary<string, object>)zencoderJobResults["job"];
                    state = ((string)zencoderJobResults["state"]).Trim().ToLowerInvariant();
                });
                //queued, processing, failed, finished, cancelled
                task.WriteLine("Zencoder job " + @params.EncoderJobId + " status: " + state);
                if (state.EqualsCaseSensitive("finished"))
                {
                    postResults = true;
                    ProcessZencoderFinishedJob(task, zencoderJobResults, @params);
                }
                else if (state.EqualsCaseInsensitive("failed") || state.EqualsCaseInsensitive("cancelled"))
                {
                    postResults = true;
                    @params.ErrorCode = TranscodeJobErrorCodes.NotCompatible;
                    zencoderJobResults["input_media_file"].IfNotNull((m) => ((IDictionary)m)["error_class"].IfNotNull((v) =>
                    {
                        (v as string).IfNotNull((error) =>
                        {
                            if (error.Trim().Equals("NoMediaError", StringComparison.OrdinalIgnoreCase))
                            {
                                @params.ErrorCode = TranscodeJobErrorCodes.NotAVideo;
                            }
                        });
                    }));
                }
                else
                {
                    wait = true;
                }

                if (postResults)
                {
                    task.Step("Posting Results to Web App", true, 5, TimeSpan.FromSeconds(30), () => { Program.CreateJsonPoster(ConfigurationManager.AppSettings["UrlPrefix"]).PostJson<ZencoderJobInProgress[]>(ProcessEncodeResultUrl, new { result = @params }); });
                }
            });
            return wait;
        }


 
        private static void ProcessZencoderFinishedJob(JobTask task, Dictionary<string, object> zencoderJobResults, EncodeJobParams jobParams)
        {
            task.Step("Creating Results", () =>
            {
                zencoderJobResults["finished_at"].IfNotNull(v => jobParams.EncodingCompleted = DateTime.Parse(Convert.ToString(v), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal));
                var outputsFromJob = (ArrayList)zencoderJobResults["output_media_files"];
                List<EncodeJobOutputVideo> outputs = new List<EncodeJobOutputVideo>();
                foreach (Dictionary<string, object> outputFromJob in outputsFromJob)
                {
                    var outputResult = new EncodeJobOutputVideo();
                    outputs.Add(outputResult);

                    var outputUrl = (string)outputFromJob["url"];
                    var outputFile = outputUrl.AfterLast(@"/");
                    outputResult.Url = outputUrl;
                    outputFromJob["label"].IfNotNull(v => outputResult.Profile = v.ToString());
                    outputFromJob["video_bitrate_in_kbps"].IfNotNull(v => outputResult.ActualVideoBitRate = v.ToString().ToIntTry().GetValueOrDefault());
                    outputFromJob["audio_bitrate_in_kbps"].IfNotNull(v => outputResult.ActualAudioBitRate = v.ToString().ToIntTry().GetValueOrDefault());
                    outputFromJob["file_size_bytes"].IfNotNull(v => outputResult.FileSize = v.ToString().ToIntTry().GetValueOrDefault());
                }
                jobParams.Outputs = outputs.ToArray();

                var thumbnailsFromJob = (ArrayList)zencoderJobResults["thumbnails"];
                if (thumbnailsFromJob != null)
                {
                    List<EncodeJobThumbnail> thumbs = new List<EncodeJobThumbnail>();
                    var processedThumbs = new HashSet<string>();
                    foreach (Dictionary<string, object> thumbFromJob in thumbnailsFromJob)
                    {
                        var s3 = new LitS3.S3Service { SecretAccessKey = ConfigurationManager.AppSettings["AmazonSecretKey"], AccessKeyID = ConfigurationManager.AppSettings["AmazonAccessKey"] };
                        //zencoder doesn't give thumbnail size or let you compress via jpeg.  so we gotta download each one, get the size, and convert to jpeg
                        task.Step("ProcessThumbnail", false, 3, TimeSpan.FromSeconds(10), () =>
                        {
                            //this chunk of shit downloads the thumbnail that zencoder made (a huge png) and converts to jpg.  also grabs the actual width/height.  
                            //NOTE: the Time is missing from the thumbanil at this point.  
                            string thumbnailUrl = (string)thumbFromJob["url"];

                            //this lame check is due to some problem where zencoder started repeating the thumbnail results twice.  since we download then delete them, the second round of downloads would fail.  so this fixes that
                            if (processedThumbs.Contains(thumbnailUrl))
                            {
                                return;
                            }
                            processedThumbs.Add(thumbnailUrl);

                            byte[] thumbnailBytes = new WebClient().DownloadData(thumbnailUrl);
                            MemoryStream inputStream = new MemoryStream(thumbnailBytes);
                            inputStream.Position = 0;
                            using (Bitmap image = new Bitmap(inputStream))
                            {
                                const long jpegQuality = 60;
                                var encoderParams = new EncoderParameters(1);
                                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);
                                MemoryStream outputStream = new MemoryStream();
                                image.Save(outputStream, JpegEncoder.Value, encoderParams);
                                outputStream.Position = 0;
                                var s3Location = S3FileLocation.FromUrl(thumbnailUrl);
                                s3.DeleteObject(s3Location.Bucket, s3Location.Key);
                                s3Location.Key = s3Location.Key.Replace(".png", ".jpg");
                                S3.UploadFileToS3AsPublicCached(outputStream, s3Location.Bucket, s3Location.Key, false);

                                thumbs.Add(new EncodeJobThumbnail
                                {
                                    Url = s3Location.Url,
                                    Height = image.Height,
                                    Width = image.Width,
                                });
                            }
                        });
                    }
                    jobParams.Thumbnails = thumbs.ToArray();
                }

            });

        }


        static Lazy<ImageCodecInfo> JpegEncoder = new Lazy<ImageCodecInfo>(() =>
        {
            return ImageCodecInfo.GetImageEncoders().Where(c => c.FormatDescription == "JPEG").First();
        });


        #endregion


    }
}
