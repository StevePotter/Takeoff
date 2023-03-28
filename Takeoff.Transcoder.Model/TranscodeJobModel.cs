using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takeoff.Transcoder
{
    /// <summary>
    /// Contains all the data surrounding a single video that was uploaded and encoded to proper format(s).
    /// </summary>
    /// <remarks>
    /// Originally there were separate "request" and "response" classes.  But this proved to be troublesome.  With introduction of advanced logging and multiple supported encoding platforms, there would be three steps: 1) request encoding 2) metadata extracted and proper encoder chosen 3) output from a specific encoder is translated to "generic" output.  In all three steps the database needs updated.  Rather than juggle all those types, I combined all the input and output into one type.  This results in lots of properties but became much easier to work with, especially when pushing around data in messages.
    /// </remarks>
    public class EncodeJobParams
    {
        /// <summary>
        /// The account id for the video being encoded.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// The video id.
        /// </summary>
        public int InputId { get; set; }

        /// <summary>
        /// An authenticated url to the input file.  
        /// </summary>
        public string InputUrl { get; set; }
        
        /// <summary>
        /// The name of the input file on the user's machine.
        /// </summary>
        public string InputOriginalFileName { get; set; }

        /// <summary>
        /// The number of bytes in the input file.
        /// </summary>
        public int InputFileSize { get; set; }

        /// <summary>
        /// Only useful for testing the system in Zencoder's "test" mode to save money.
        /// </summary>
        public bool? Test { get; set; }

        /// <summary>
        /// Indicates an error that occured during encoding.
        /// </summary>
        public TranscodeJobErrorCodes ErrorCode { get; set; }

        /// <summary>
        /// The encoding platform that does the actual encoding.  Currently this is only "zencoder" or "panda".
        /// </summary>
        public string Encoder { get; set; }

        /// <summary>
        /// The ID returned from the API call to the encoder.
        /// </summary>
        public string EncoderJobId { get; set; }

        /// <summary>
        /// The maximum number of seconds that are allowed, or else ErrorCode will be set to DurationTooLong
        /// </summary>
        public int? DurationLimit { get; set; }

        /// <summary>
        /// When a message was sent to our encoding triage job.  That job runs on EC2 and gets metadata, chooses an encoder, and makes the call to the encoder.
        /// </summary>
        public System.Nullable<System.DateTime> EncodingTriageRequested { get; set; }

        /// <summary>
        /// When the message sent on EncodingTriageRequested was picked up by the EC2 job.  
        /// </summary>
        public System.Nullable<System.DateTime> EncodingTriageStarted { get; set; }

        /// <summary>
        /// When an API call to the encoding platform was made.
        /// </summary>
        public System.Nullable<System.DateTime> EncodingRequested { get; set; }

        /// <summary>
        /// When the encoder completed its work.
        /// </summary>
        public System.Nullable<System.DateTime> EncodingCompleted { get; set; }

        /// <summary>
        /// Indicates when the entire job has finally completed.
        /// </summary>
        public System.Nullable<System.DateTime> JobCompleted { get; set; }

        /// <summary>
        /// The various output formats for encoding.
        /// </summary>
        public EncodeJobOutputVideo[] Outputs { get; set; }

        /// <summary>
        /// The duration of the video, in seconds, extracted from meta data.
        /// </summary>
        public double? InputDuration { get; set; }

        /// <summary>
        /// The format of the uploaded file, such as "mp4".
        /// </summary>
        public string InputFileFormat { get; set; }

        /// <summary>
        /// The codec used to encode video.
        /// </summary>
        public string InputVideoFormat { get; set; }

        public double? InputFrameRate { get; set; }

        public int? InputWidth { get; set; }

        public int? InputHeight { get; set; }

        public int? InputVideoBitRate { get; set; }

        public string InputAudioFormat { get; set; }

        /// <summary>
        /// The audio bitrate, in kps, of the input file.
        /// </summary>
        public int? InputAudioBitRate { get; set; }

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
        /// If ThumbnailAcl is public, this indicates whether to add public non-expiring cache headers so the thumbnail can be cached by browsers.
        /// </summary>
        public bool AddNonExpireCacheIfThumbnailAclPublic { get; set; }

        /// <summary>
        /// The s3 bucket and folder to upload the thumbnails to.
        /// </summary>
        public string ThumbnailLocation { get; set; }

        /// <summary>
        /// The thumbnails that were generated for the source video.
        /// </summary>
        public EncodeJobThumbnail[] Thumbnails { get; set; }

    }

    public class EncodeJobOutputVideo
    {

        /// <summary>
        /// This is the desired url, in s3 format, for the output file.  It is up to specific encoding interfaces to adjust the actual url sent.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Input param.  The name of the profile used, such as "web" or "mobile"
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// Output param.  Indicates the number of bytes in the target file.  
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// When set, this is the width of the video.  
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// When set, this is the height of the video.  
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// The quality level of the video.  Valid numbers are 1-5, and this is only for Zencoder at the moment.
        /// </summary>
        public int? TargetVideoQuality { get; set; }

        /// <summary>
        /// Exclusive of TargetVideoQuality.  Gives the desired video bitrate in KB/sec.
        /// </summary>
        public int? TargetVideoBitrate { get; set; }

        /// <summary>
        /// Exclusive of TargetVideoQuality.  Gives the desired video bitrate in KB/sec.
        /// </summary>
        public int? TargetMaxVideoBitrate { get; set; }

        /// <summary>
        /// Set after the video has been encoded.  The actual bit rate, in KB/sec, of the outputted video.  Examples: 500, 1400 (1.4 Mbps)
        /// </summary>
        public int ActualVideoBitRate { get; set; }

        /// <summary>
        /// The quality level of the audio.  Valid numbers are 1-5, and this is only for Zencoder at the moment.
        /// </summary>
        public int? TargetAudioQuality { get; set; }

        /// <summary>
        /// Exclusive of TargetAudioQuality.  Gives the desired audio bitrate in KB/sec.
        /// </summary>
        public int? TargetAudioBitrate { get; set; }

        /// <summary>
        /// Set after the audio has been encoded.  The actual bit rate, in KB/sec, of the outputted audio.  Examples: 500, 1400 (1.4 Mbps)
        /// </summary>
        public int ActualAudioBitRate { get; set; }

        /// <summary>
        /// The sample rate to encode audio at.
        /// </summary>
        public int? AudioSampleRate { get; set; }

        /// <summary>
        /// Input param.  The access level of the video in S3.
        /// </summary>
        public Amazon.S3.S3CannedACL TargetAcl { get; set; }

        /// <summary>
        /// Output param. The video codec that was used.
        /// </summary>
        public string VideoCodec { get; set; }

        /// <summary>
        /// The audio codec to use. 
        /// </summary>
        public string AudioCodec { get; set; }

        /// <summary>
        /// If a video's frame rate exceeds this number, it will get encoded at this frame rate.
        /// </summary>
        public int? MaxFrameRate { get; set; }

        /// <summary>
        /// Input param.  If greater than zero, key frame intervals (aka GOP loength) will be adjusted to reach a certain number of key frames per second of video.
        /// </summary>
        /// <remarks>
        /// This is not guaranteed to be perfectly accurate because GOP size is in clumps of frames, so for example, it's impossible to get 3 key frames per second perfect if the frame rate is 25, since they don't divide evenly.
        /// </remarks>
        public int KeyFramesPerSecond { get; set; }

        /// <summary>
        /// Number of frames in between key frames.  AKA GOP size.
        /// </summary>
        public int KeyFrameInterval { get; set; }

        /// <summary>
        /// Every x frames or every x seconds.  Ex: 1f, 10f, 0.2s, 5s
        /// </summary>
        public string KeyFrameFrequency { get; set; }

    }

    /// <summary>
    /// Contains information about a thumbnail that was generated for video.
    /// </summary>
    public class EncodeJobThumbnail
    {
        /// <summary>
        /// The file name of the resulting file. 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The width, in pixels, of the thumbnail.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height, in pixels, of the thumbnail.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The video time, in seconds, that the thumbnail was generated from.
        /// </summary>
        public double Time { get; set; }
    }


























    
    ///// <summary>
    ///// Contains information about a thumbnail that was generated for video.
    ///// </summary>
    //public class TranscodedVideoThumbnail
    //{
    //    /// <summary>
    //    /// The file name of the resulting file.  This is not set if transcoding is skipped.
    //    /// </summary>
    //    public string FileName { get; set; }

    //    /// <summary>
    //    /// The s3 bucket and folder the transcoded video is in.  This is not set if transcoding is skipped.
    //    /// </summary>
    //    public string FileLocation { get; set; }

    //    /// <summary>
    //    /// The width, in pixels, of the thumbnail.
    //    /// </summary>
    //    public int Width { get; set; }

    //    /// <summary>
    //    /// The height, in pixels, of the thumbnail.
    //    /// </summary>
    //    public int Height { get; set; }

    //    /// <summary>
    //    /// The video time, in seconds, that the thumbnail was generated from.
    //    /// </summary>
    //    public double Time { get; set; }
    //}



    ///// <summary>
    ///// Contains everything needed by the transcoding server to transcode one source video into multiple targets.
    ///// This is used when we transcode using our own server, which will be obsolete once we switch to another encoding provider.
    ///// </summary>
    ///// <remarks>
    ///// Code shared between Takeoff web server and transcode server.  Keep code in sync.
    ///// </remarks>
    //public class TranscodeJobParameters
    //{
    //    /// <summary>
    //    /// The id of this job as set by the requesting system.  This currently maps to a table in Takeoff database.
    //    /// </summary>
    //    public string JobId { get; set; }

    //    /// <summary>
    //    /// The S3 bucket and folder of the source video.
    //    /// </summary>
    //    public string SourceLocation { get; set; }

    //    /// <summary>
    //    /// The file name (not necessarily S3 key) of the source video.
    //    /// </summary>
    //    public string SourceFileName { get; set; }

    //    /// <summary>
    //    /// The url for that will be called on the MVC app with the message body.  Note the action must take a single string parameter called "message" that is the json string for the result.
    //    /// </summary>
    //    public string CallbackActionUrl { get; set; }

    //    /// <summary>
    //    /// 1-x target encodings of the source video.  
    //    /// </summary>
    //    public TranscodeTargetParameters[] Targets { get; set; }

    //    /// <summary>
    //    /// The ideal number of thumbnails to generate.
    //    /// </summary>
    //    public int Thumbnails { get; set; }

    //    /// <summary>
    //    /// The max available width for the thumbnail.  The actual width could be less.
    //    /// </summary>
    //    public int ThumbnailMaxWidth { get; set; }

    //    /// <summary>
    //    /// The max available height for the thumbnail.  The actual height could be less.
    //    /// </summary>
    //    public int ThumbnailMaxHeight { get; set; }    

    //    /// <summary>
    //    /// The access level of thumbnails in S3.
    //    /// </summary>
    //    public Amazon.S3.S3CannedACL ThumbnailAcl { get; set; }

    //    /// <summary>
    //    /// If ThumbnailAcl is public, this indicates whether to add public non-expiring cache headers so the thumbnail can be cached by browsers.
    //    /// </summary>
    //    public bool AddNonExpireCacheIfThumbnailAclPublic { get; set; }

    //    /// <summary>
    //    /// The s3 bucket and folder to upload the thumbnails to.
    //    /// </summary>
    //    public string ThumbnailLocation { get; set; }    
    //}

    ///// <summary>
    ///// Contains parameters for encoding the source video into a single target video.  Each job can have 1-x targets.
    ///// </summary>
    //public class TranscodeTargetParameters
    //{
    //    /// <summary>
    //    /// The access level of the video in S3.
    //    /// </summary>
    //    public Amazon.S3.S3CannedACL TargetAcl { get; set; }

    //    /// <summary>
    //    /// The s3 bucket and folder to upload the transcoded video to.
    //    /// </summary>
    //    public string TargetLocation { get; set; }

    //    /// <summary>
    //    /// Indicates the extension for the generated target file.  Default is "mp4".  
    //    /// </summary>
    //    public string FileExtension { get; set; }

    //    /// <summary>
    //    /// The container format for the file ("-f" option in FFMpeg).
    //    /// </summary>
    //    public string Format { get; set; }

    //    /// <summary>
    //    /// The video codec to use. 
    //    /// </summary>
    //    public string VideoCodec { get; set; }

    //    /// <summary>
    //    /// The target bit rate, in KB/sec, for the video.  Examples: 500, 1400 (1.4 Mbps)
    //    /// </summary>
    //    public int VideoBitRate { get; set; }

    //    /// <summary>
    //    /// The audio codec to use. 
    //    /// </summary>
    //    public string AudioCodec { get; set; }

    //    /// <summary>
    //    /// The target bit rate, in KB/sec, for the audio.
    //    /// </summary>
    //    public int AudioBitRate { get; set; }

    //    /// <summary>
    //    /// Additional custom arguments passed to the transcoder (taken from presets), such as x264 params.
    //    /// </summary>
    //    public string TranscoderArgs { get; set; }

    //    /// <summary>
    //    /// The name of the preset file (for ffmpeg it's stuff like "libx264-normal").
    //    /// </summary>
    //    public string Preset { get; set; }

    //    /// <summary>
    //    /// Frames per second.  Values like 10, 15, 25, 29.997, 30.  Default depends on UseSourceFrameRateIfLower, but will fall back to 30.
    //    /// </summary>
    //    public double FrameRate { get; set; }

    //    /// <summary>
    //    /// If greater than zero, key frame intervals (aka GOP loength) will be adjusted to reach a certain number of key frames per second of video.
    //    /// </summary>
    //    /// <remarks>
    //    /// This is not guaranteed to be perfectly accurate because GOP size is in clumps of frames, so for example, it's impossible to get 3 key frames per second perfect if the frame rate is 25, since they don't divide evenly.
    //    /// </remarks>
    //    public int KeyFramesPerSecond { get; set; }

    //    /// <summary>
    //    /// The width of the video frame, such as 1280.  Default is same as source.
    //    /// </summary>
    //    public int Width { get; set; }

    //    /// <summary>
    //    /// The height of the video frame, such as 720.  Default is same as source.
    //    /// </summary>
    //    public int Height { get; set; }

    //    /// <summary>
    //    /// If the source video has a lower frame rate than FrameRate, this indicates whether we use the source frame rate and not the FrameRate value.
    //    /// </summary>
    //    public bool UseSourceFrameRateIfLower { get; set; }

    //    /// <summary>
    //    /// If the video stream is supported by Flash and its bitrate isn't greater than this value, the video will be skipped.  This means it will be copied if ffmpeg runs, or if the audio can also be skipped, transcoding will be skipped entirely.
    //    /// </summary>
    //    /// <remarks>Note that if video is skipped, bit rate, frame rate, keyframes etc will not apply.</remarks>
    //    public int MaxSkipVideoBitRate { get; set; }

    //    /// <summary>
    //    /// If the audio stream is supported by Flash and its bitrate isn't greater than this value, the audio will be skipped.  This means it will be copied if ffmpeg runs, or if the video can also be skipped, transcoding will be skipped entirely.
    //    /// </summary>
    //    /// <remarks>Note that if audio is skipped, bit rate, sample rate, etc will not apply.</remarks>
    //    public int MaxSkipAudioBitRate { get; set; }

    //    /// <summary>
    //    /// If the source is streamable but has bit rates greater than MaxSkipVideoBitRate or MaxSkipAudioBitRate, it will 
    //    /// be run through the transcoder.  However, sometimes the result is actually bigger than the source (especially for h264 that was encoded slightly above the bit rate max).
    //    /// In that case, this property indicates that if the transcoded file is bigger than the source, even though its bit rate is above the max, it will be skipped.
    //    /// </summary>
    //    public bool SkipIfResultIsBiggerThanSource { get; set; }


    //}

    ///// <summary>
    ///// Contains information the server needs from a completed or failed transcoding job.
    ///// </summary>
    //public class TranscodeJobResult
    //{
    //    /// <summary>
    //    /// The id of this job as set by the requesting system.  This currently maps to a table in Takeoff database.
    //    /// </summary>
    //    public string JobId { get; set; }

    //    /// <summary>
    //    /// Contains information about the error, if any, that occured during the job.
    //    /// </summary>
    //    public TranscodeJobErrorCodes ErrorCode { get; set; }

    //    /// <summary>
    //    /// The url for that will be called on the MVC app with the message body.  Note the action must take a single string parameter called "message" that is the json string for the result.
    //    /// </summary>
    //    public string CallbackActionUrl { get; set; }

    //    /// <summary>
    //    /// Results of the 1-x target encodings of the source video.  
    //    /// </summary>
    //    public TranscodeTargetResult[] Targets { get; set; }

    //    /// <summary>
    //    /// The thumbnails that were generated for the source video.
    //    /// </summary>
    //    public TranscodedVideoThumbnail[] Thumbnails { get; set; }

    //    /// <summary>
    //    /// The encoder that was selected to do the processing.
    //    /// </summary>
    //    public string Encoder { get; set; }

    //    /// <summary>
    //    /// The ID that hooks this job up with the external encoder's system.
    //    /// </summary>
    //    public string EncoderJobId { get; set; }

    //    /// <summary>
    //    /// Indicates the exact time the encoding job completed.  Soon after this the system should update.
    //    /// </summary>
    //    public DateTime? EncodingCompleted { get; set; }

    //}

    ///// <summary>
    ///// Contains information about single transcoded video stream for a source video.
    ///// </summary>
    //public class TranscodeTargetResult
    //{
    //    /// <summary>
    //    /// The name of the profile used, such as "Web" or "Mobile".  
    //    /// </summary>
    //    public string Profile { get; set; }

    //    /// <summary>
    //    /// The file name of the resulting file.  This is not set if transcoding is skipped.
    //    /// </summary>
    //    public string FileName { get; set; }

    //    /// <summary>
    //    /// The s3 bucket and folder the transcoded video is in.  This is not set if transcoding is skipped.
    //    /// </summary>
    //    public string FileLocation { get; set; }

    //    /// <summary>
    //    /// Indicates the number of bytes in the target file.  This is not set if transcoding is skipped.
    //    /// </summary>
    //    public int FileSize { get; set; }

    //    /// <summary>
    //    /// The target bit rate, in KB/sec, for the video.  Examples: 500, 1400 (1.4 Mbps)
    //    /// </summary>
    //    public int VideoBitRate { get; set; }

    //    /// <summary>
    //    /// The target bit rate, in KB/sec, for the audio.
    //    /// </summary>
    //    public int AudioBitRate { get; set; }

    //    /// <summary>
    //    /// If true, the transcoding was skipped because the video is already web ready.  In this case the source video can be streamed.  No target files are uploaded.
    //    /// </summary>
    //    public bool TranscodingSkipped { get; set; }
    //}


    /// <summary>
    /// Error codes for a transcode job.
    /// </summary>
    public enum TranscodeJobErrorCodes
    {
        /// <summary>
        /// No error.  Success!
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// File was not a video.
        /// </summary>
        NotAVideo = 1,
        /// <summary>
        /// The file was a video but couldn't be transcoded.
        /// </summary>
        NotCompatible = 2,
        /// <summary>
        /// Some other error.  These are probably on our end or someone tampered with input.
        /// </summary>
        Other = 3,
        /// <summary>
        /// The duration was longer than the limit set.
        /// </summary>
        DurationTooLong = 4,

    }

}

