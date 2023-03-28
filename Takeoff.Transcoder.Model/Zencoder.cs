using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;

namespace Takeoff.Transcoder
{

    public class ZencoderClient
    {
        public static string ApiBaseUrl = "https://app.zencoder.com/api/v2";

        public Dictionary<string, object> CreateJob(ZencoderCreateJobRequest request)
        {
            var poster = new JsonPoster(ApiBaseUrl);

            var job = new Dictionary<string, object>();
            job["api_key"] = request.ApiKey;
            if (request.Test.GetValueOrDefault())
                job["test"] = 1;
            job["input"] = request.Input;
            if (request.Region.HasChars())
                job["region"] = request.Region;

            ArrayList outputs = new ArrayList();
            job["outputs"] = outputs;

            if (request.NotificationUrl.HasChars())
            {
                ArrayList notifications = new ArrayList();
                notifications.Add(request.NotificationUrl);
                job["notifications"] = notifications;
            }

            bool isFirst = true;
            foreach (var outputRequest in request.Outputs)
            {
                var output = new Dictionary<string, object>();
                outputs.Add(output);

                output["url"] = outputRequest.TargetUrl;
                outputRequest.VideoQuality.IfHasVal(v => output["quality"] = v);
                outputRequest.VideoBitrate.IfHasVal(v => output["video_bitrate"] = v);
                outputRequest.MaxBitrate.IfHasVal(v => output["bitrate_cap"] = v);

                outputRequest.AudioQuality.IfHasVal(v => output["audio_quality"] = v);
                outputRequest.AudioBitrate.IfHasVal(v => output["audio_bitrate"] = v);
                outputRequest.AudioSampleRate.IfHasVal(v => output["audio_sample_rate"] = v);

                outputRequest.Label.IfNotNull(v => output["label"] = v);

                outputRequest.MaxFramerate.IfHasVal(v => output["max_frame_rate"] = v);
                outputRequest.KeyFrameInterval.IfHasVal(v => output["keyframe_interval"] = v);
                outputRequest.KeyFramesPerSecond.IfHasVal(v => output["keyframe_rate"] = v);
                
                outputRequest.Width.IfHasVal(v => output["width"] = v);
                outputRequest.Height.IfHasVal(v => output["height"] = v);


                if (isFirst)
                {
                    isFirst = false;//don't do two sets of thumbnails or else we get double the results in the resulting api call
                    if (request.ThumbnailCount > 0)
                    {
                        var thumbOptions = new Dictionary<string, object>();
                        output["thumbnails"] = thumbOptions;
                        thumbOptions["number"] = request.ThumbnailCount;
                        if (request.ThumbnailMaxWidth.IsPositive() && request.ThumbnailMaxHeight.IsPositive())
                        {
                            thumbOptions["size"] = request.ThumbnailMaxWidth.ToInvariant() + "x" + request.ThumbnailMaxHeight.ToInvariant();
                        }
                        request.ThumbnailBaseUrl.IfNotNull(v => thumbOptions["base_url"] = v);
                        request.ThumbnailPrefix.IfNotNull(v => thumbOptions["prefix"] = v);
                        if (request.ThumbnailAcl == Amazon.S3.S3CannedACL.PublicRead)
                            thumbOptions["public"] = 1;
                    }
                }
            }

            poster.PostJson("/jobs", job);
            return (Dictionary<string, object>)poster.DeserializeResponseJson();

        }
 

    }


    /// <summary>
    /// Contains data to make a single request to zencoder.com.
    /// </summary>
    /// <remarks>
    /// http://zencoder.com/docs/api/
    /// </remarks>
    public class ZencoderCreateJobRequest
    {

        /// <summary>
        ///API key for the account.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// When true, the request is just a test and only 5 secs will be encoded.
        /// </summary>
        public bool? Test { get; set; }

        /// <summary>
        /// The url to the input file.
        /// </summary>
        public string Input { get; set; }

        public string NotificationUrl { get; set; }

        /// <summary>
        /// The desired region to encode in.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// 1-x target encodings of the source video.  
        /// </summary>
        public ZencoderOutputParams[] Outputs { get; set; }

        /// <summary>
        /// The ideal number of thumbnails to generate.
        /// </summary>
        public int ThumbnailCount { get; set; }

        /// <summary>
        /// The max available width for the thumbnail.  The actual width could be less.
        /// </summary>
        public int ThumbnailMaxWidth { get; set; }

        /// <summary>
        /// The max available height for the thumbnail.  The actual height could be less.
        /// </summary>
        public int ThumbnailMaxHeight { get; set; }    

        /// <summary>
        /// The access level of thumbnails in S3.
        /// </summary>
        public Amazon.S3.S3CannedACL ThumbnailAcl { get; set; }

        /// <summary>
        /// The s3 bucket and folder to upload the thumbnails to.  Ex: s3://takeoff-data/1234
        /// </summary>
        public string ThumbnailBaseUrl { get; set; }

        /// <summary>
        /// The file name prefix for the thumbnails.
        /// </summary>
        public string ThumbnailPrefix { get; set; } 


    }

    /// <summary>
    /// Contains parameters for encoding the source video into a single target video.  Each job can have 1-x targets.
    /// </summary>
    public class ZencoderOutputParams
    {
        /// <summary>
        /// The url (s3) to be uploaded to.  Ex: s3://takeoff-data/1234/asdf.mp4.  Zencoder must have write permissions on the bucket.
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// When set, this is the width of the video.  
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// When set, this is the width of the video.  
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// The quality level of the video.  Valid numbers are 1-5.
        /// </summary>
        public int? VideoQuality { get; set; }

        /// <summary>
        /// Exclusive of VideoQuality.
        /// </summary>
        public int? VideoBitrate { get; set; }
        
        /// <summary>
        /// Max bitrate.
        /// </summary>
        public int? MaxBitrate { get; set; }
        
        /// <summary>
        /// The quality level of the audio.  Valid numbers are 1-5.
        /// </summary>
        public int? AudioQuality { get; set; }

        /// <summary>
        /// Exclusive of AudioQuality.
        /// </summary>
        public int? AudioBitrate { get; set; }

        public int? AudioSampleRate { get; set; }

        public int? MaxFramerate { get; set; }

        /// <summary>
        /// If greater than zero, key frame intervals (aka GOP loength) will be adjusted to reach a certain number of key frames per second of video.
        /// </summary>
        /// <remarks>
        /// This is not a zencoder parameter and instead is calculated by our transcoding server once metadata is grabbed via mediainfo.
        /// </remarks>
        public int? KeyFramesPerSecond { get; set; }

        
        public int? KeyFrameInterval { get; set; }


        /// <summary>
        /// Optional identifier for the output.  Such as "mobile" or "web"
        /// </summary>
        public string Label { get; set; }

    }

    /// <summary>
    /// Sent back after the file has been analyzed and a possible zencoder request has been made.
    /// </summary>
    public class ZencoderCreateJobResponse
    {
        /// <summary>
        /// The id of this job as set by the requesting system.  This currently maps to a table in Takeoff database.
        /// </summary>
        public string TakeoffJobId { get; set; }

        /// <summary>
        /// The id of this job as returned by zencoder.
        /// </summary>
        public int ZencoderJobId { get; set; }

        /// <summary>
        /// Contains information about the error, if any, that occured during the job.
        /// </summary>
        public TranscodeJobErrorCodes ErrorCode { get; set; }

        /// <summary>
        /// The json object returned from the zencoder api.  This will be null if no request was made.
        /// </summary>
        public Dictionary<string, object> ZencoderResponse { get; set; }

        /// <summary>
        /// The duration, in seconds, of the source video.  Useful for reporting purposes.
        /// </summary>
        public double SourceDuration { get; set; }

        /// <summary>
        /// The number of seconds it took to process the job.
        /// </summary>
        public int JobDuration { get; set; }
    }


    public class ZencoderJobInProgress
    {
        public int VideoId { get; set; }
        public int TakeoffJobId { get; set; }
        public string ZencoderJobId { get; set; }
    }
    
}
