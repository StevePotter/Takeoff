using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Amazon.S3.Model;
using Mediascend.Web;
using MvcContrib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Takeoff.Data;
using Takeoff.Models;
using System.Text;
using System.Xml.Linq;
using Takeoff.Transcoder;
using Takeoff.Platform;
using System.IO;
using Takeoff.ViewModels;

namespace Takeoff.Controllers
{
    public class ZencoderNotification
    {
        public ZencoderNotificationJob Job { get; set; }
    }

    public class ZencoderNotificationJob
    {
        public ZencoderJobState State { get; set; }
        public string Id { get; set; }
    }

    public enum ZencoderJobState
    {
        NotSet,
        Processing,
        Finished,
        Failed,
        Cancelled,
    }


//#todo: request a notification.  ensure they are being read back properly.  get output from zencoder.  update DB accordingly.  move from png to jpg thumbs

    public class EncodesController : BasicController
    {
        /// <summary>
        /// The url format string to the GET api that will return a job's status.
        /// </summary>
        private static string ZencoderJobStatusUrl = "https://app.zencoder.com/api/jobs/{0}?api_key={1}";

        private static string ZencoderApiKey = ConfigurationManager.AppSettings["ZencoderApiKey"];


        public ActionResult ZencoderNotification(ZencoderNotification notification)
        {
            if (notification == null || notification.Job == null || notification.Job.State == ZencoderJobState.NotSet)
                return this.StatusCode(HttpStatusCode.BadRequest);
            //ignore until it's done
            if (notification.Job.State == ZencoderJobState.Processing )
                return this.Content("Ignoring processing state.");

            EncodeJobParams encodeJob = null;
            //first thing we do is check to make sure the video hasn't already been marked as completed or screwed up somehow.  this happened a bit in the past, and ideally wouldn't happen in a perfect world
            using (var db = DataModel.ReadOnly)
            {
                var log = (from tj in db.EncodeLogs where tj.EncoderJobId == notification.Job.Id select tj).SingleOrDefault();
                if (log == null)
                    return this.StatusCode(HttpStatusCode.NotFound);
                if (log.JobCompleted.HasValue)
                    return this.Content("Ignoring job that was already completed.");

                encodeJob = new EncodeJobParams {InputUrl = log.InputUrl, InputId = log.InputId, Encoder = log.Encoder, EncoderJobId = log.EncoderJobId};
            }

            var video = Things.GetOrNull<VideoThing>(encodeJob.InputId);
            ProjectThing production = video == null ? null : video.FindAncestor<ProjectThing>();
            FileThing videoFile = video == null ? null : video.FindChild<FileThing>();

            //if production or video is null, they deleted it before the transcoding completed.
            if (video == null)
            {
                return this.Empty();//you really should delete the files from S3 but it's so rare it won't affect much
            }


            Dictionary<string, object> zencoderJobResults = null;
            var jobStatusUrl = string.Format(ZencoderJobStatusUrl, notification.Job.Id, ZencoderApiKey);
            zencoderJobResults = new JsonPoster().GetJson<Dictionary<string, object>>(jobStatusUrl);
            zencoderJobResults = (Dictionary<string, object>)zencoderJobResults["job"];

            if (notification.Job.State != ZencoderJobState.Finished)
            {
                if (notification.Job.State == ZencoderJobState.Failed)
                {
                    var errorCode = TranscodeJobErrorCodes.NotCompatible;
                    zencoderJobResults["input_media_file"].IfNotNull((m) => ((IDictionary)m)["error_class"].IfNotNull((v) =>
                    {
                        (v as string).IfNotNull((error) =>
                        {
                            if (error.Trim().Equals("NoMediaError", StringComparison.OrdinalIgnoreCase))
                            {
                                errorCode = TranscodeJobErrorCodes.NotAVideo;
                            }
                        });
                    }));

                    this.DeferRequest<EmailController>(c => c.SendProductionVideoTranscodeError(video.CreatedByUserId, production.Id, videoFile.OriginalFileName, errorCode, video.Title));
                }

                video.TrackChanges();
                video.IsComplimentary = true;//this is necessary to ensure Account.VideosAddedInBillingCycle is right.  
                video.Update();
                video.Delete();
                return this.Content("Video was deleted prior to encoding or an error occured.");
            }

            //from here out is only a finished job
            zencoderJobResults["finished_at"].IfNotNull(v => encodeJob.EncodingCompleted = DateTime.Parse(Convert.ToString(v), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal));

            using (var insertBatcher = new CommandBatcher())
            {
                var now = this.RequestDate();
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

                    var videoStream = video.AddChild(new VideoStreamThing
                    {
                        CreatedByUserId = video.CreatedByUserId,
                        CreatedOn = now,
                        VideoBitRate = outputResult.ActualVideoBitRate,
                        AudioBitRate = outputResult.ActualAudioBitRate,
                        DeletePhysicalFile = true,
                        Profile = outputResult.Profile,
                    });

                    videoStream.Url = outputResult.Url;
                    videoStream.Bytes = outputResult.FileSize;
                    videoStream.QueueInsertData(insertBatcher);
                
                }
                encodeJob.Outputs = outputs.ToArray();


                var thumbnailsFromJob = (ArrayList)zencoderJobResults["thumbnails"];
                if (thumbnailsFromJob != null)
                {
                    List<EncodeJobThumbnail> thumbs = new List<EncodeJobThumbnail>();
                    var processedThumbs = new HashSet<string>();
                    foreach (Dictionary<string, object> thumbFromJob in thumbnailsFromJob)
                    {
                        var s3 = new LitS3.S3Service { SecretAccessKey = ConfigurationManager.AppSettings["AmazonSecretKey"], AccessKeyID = ConfigurationManager.AppSettings["AmazonAccessKey"] };
                        //this chunk of shit downloads the thumbnail that zencoder made (a huge png) and converts to jpg.  also grabs the actual width/height.  
                        //NOTE: the Time is missing from the thumbanil at this point.  
                        string thumbnailUrl = (string)thumbFromJob["url"];

                        //this lame check is due to some problem where zencoder started repeating the thumbnail results twice.  since we download then delete them, the second round of downloads would fail.  so this fixes that
                        if (processedThumbs.Contains(thumbnailUrl))
                        {
                            continue;
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
                            EncodeJobThumbnail thumbnail = new EncodeJobThumbnail
                                                                {
                                                                    Url = s3Location.Url,
                                                                    Height = image.Height,
                                                                    Width = image.Width,
                                                                };
                            thumbs.Add(thumbnail);
                            video.AddChild(new VideoThumbnailThing
                            {
                                CreatedByUserId = video.CreatedByUserId,
                                CreatedOn = now,
                                Url = thumbnail.Url,
                                Time = thumbnail.Time,
                                Width = thumbnail.Width,
                                Height = thumbnail.Height,
                                DeletePhysicalFile = true,
                                LogInsertActivity = false,
                            }).QueueInsertData(insertBatcher);

                        }
                    }
                    encodeJob.Thumbnails = thumbs.ToArray();
                }


                insertBatcher.Execute();

                video.TrackChanges();

                //log the duration
                if (zencoderJobResults["input_media_file"] != null)
                {
                    IDictionary inputFile = (IDictionary)zencoderJobResults["input_media_file"];
                    if (inputFile["duration_in_ms"] != null)
                    {
                        TimeSpan duration = TimeSpan.FromMilliseconds(Convert.ToInt32(inputFile["duration_in_ms"]));
                        video.Duration = duration;
                        encodeJob.InputDuration = duration.TotalSeconds;
                    }
                }
                var changes = new Dictionary<string, object>();
                changes["HasVideo"] = video.HasVideo;
                changes["Thumbnails"] = video.Children.OfType<VideoThumbnailThing>().OrderBy(t => t.Time).Select(c => (VideoThumbnailThingView)c.CreateViewData(this.Identity())).ToArray();
                video.Update(true, null, changes);
            }

            this.DeferRequest<EmailController>(c => c.NotifyForNewVideo(video.Id));
        
            encodeJob.JobCompleted = DateTime.UtcNow;
            Videos.UpdateEncodeLog(encodeJob);

            return this.Content("Done!");
        }


        static Lazy<ImageCodecInfo> JpegEncoder = new Lazy<ImageCodecInfo>(() =>
        {
            return ImageCodecInfo.GetImageEncoders().Where(c => c.FormatDescription == "JPEG").First();
        });


      
    }
}
       