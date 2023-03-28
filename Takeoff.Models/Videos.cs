using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Takeoff.Transcoder;

namespace Takeoff.Models
{
    /// <summary>
    /// Business logic for videos.
    /// </summary>
    public static class Videos
    {
        public static EncodeJobParams[] VideosBeingEncoded()
        {
            using (var db = DataModel.ReadOnly)
            {
                var jobs = (from j in db.EncodeLogs
                            where j.JobCompleted == null && j.EncodingRequested != null && j.EncodingCompleted == null
                            select new EncodeJobParams { InputUrl = j.InputUrl, InputId = j.InputId, Encoder = j.Encoder, EncoderJobId = j.EncoderJobId })
                    .FilterDeletedThings(db, p => p.InputId).ToArray();
                return jobs.HasItems() ? jobs : null;
            }
        }


        /// <summary>
        /// Updates the log data kept for a particular video encode.
        /// </summary>
        /// <param name="jobParams"></param>
        /// <returns></returns>
        public static Models.Data.EncodeLog UpdateEncodeLog(EncodeJobParams jobParams)
        {
            using (var db = new DataModel())
            {
                var data = (from t in db.EncodeLogs where t.InputId == jobParams.InputId select t).Single();

                if (jobParams.AccountId.IsPositive() && jobParams.AccountId != data.AccountId)
                    data.AccountId = jobParams.AccountId;
                if (jobParams.InputAudioBitRate.HasValue && jobParams.InputAudioBitRate != data.InputAudioBitRate)
                    data.InputAudioBitRate = jobParams.InputAudioBitRate;
                if (jobParams.InputAudioFormat.HasChars() && !jobParams.InputAudioFormat.EqualsCaseSensitive(data.InputAudioFormat))
                    data.InputAudioFormat = jobParams.InputAudioFormat;
                if (jobParams.InputFileSize.IsPositive() && jobParams.InputFileSize != data.InputBytes)
                    data.InputBytes = jobParams.InputFileSize;
                if (jobParams.Encoder.HasChars() && !jobParams.Encoder.EqualsCaseSensitive(data.Encoder))
                    data.Encoder = jobParams.Encoder;
                if (jobParams.EncoderJobId.HasChars() && !jobParams.EncoderJobId.EqualsCaseSensitive(data.EncoderJobId))
                    data.EncoderJobId = jobParams.EncoderJobId;
                if (jobParams.EncodingCompleted.HasValue && jobParams.EncodingCompleted != data.EncodingCompleted)
                    data.EncodingCompleted = jobParams.EncodingCompleted;
                if (jobParams.EncodingRequested.HasValue && jobParams.EncodingRequested != data.EncodingRequested)
                    data.EncodingRequested = jobParams.EncodingRequested;
                if (jobParams.EncodingTriageRequested.HasValue && jobParams.EncodingTriageRequested != data.EncodingTriageRequested)
                    data.EncodingTriageRequested = jobParams.EncodingTriageRequested;
                if (jobParams.EncodingTriageStarted.HasValue && jobParams.EncodingTriageStarted != data.EncodingTriageStarted)
                    data.EncodingTriageStarted = jobParams.EncodingTriageStarted;
                if (jobParams.ErrorCode != TranscodeJobErrorCodes.NotSet && (int)jobParams.ErrorCode != data.ErrorCode.GetValueOrDefault())
                    data.ErrorCode = (int)jobParams.ErrorCode;
                if (jobParams.InputFileFormat.HasChars() && !jobParams.InputFileFormat.EqualsCaseSensitive(data.InputFileFormat))
                    data.InputFileFormat = jobParams.InputFileFormat;
                if (jobParams.JobCompleted.HasValue && jobParams.JobCompleted != data.JobCompleted)
                    data.JobCompleted = jobParams.JobCompleted;
                if (jobParams.InputOriginalFileName.HasChars() && !jobParams.InputOriginalFileName.EqualsCaseSensitive(data.InputOriginalFileName))
                    data.InputOriginalFileName = jobParams.InputOriginalFileName;
                if (jobParams.Outputs != null && jobParams.Outputs.Length != data.Outputs.GetValueOrDefault())
                    data.Outputs = jobParams.Outputs.Length;
                if (jobParams.InputUrl.HasChars() && !jobParams.InputUrl.EqualsCaseSensitive(data.InputUrl))
                    data.InputUrl = jobParams.InputUrl;
                if (jobParams.InputVideoBitRate.HasValue && jobParams.InputVideoBitRate != data.InputVideoBitRate)
                    data.InputVideoBitRate = jobParams.InputVideoBitRate;
                if (jobParams.InputDuration.HasValue && jobParams.InputDuration != data.InputDuration)
                    data.InputDuration = jobParams.InputDuration;
                if (jobParams.InputVideoFormat.HasChars() && !jobParams.InputVideoFormat.EqualsCaseSensitive(data.InputVideoFormat))
                    data.InputVideoFormat = jobParams.InputVideoFormat;
                if (jobParams.InputFrameRate.HasValue && jobParams.InputFrameRate != data.InputFrameRate)
                    data.InputFrameRate = jobParams.InputFrameRate;
                if (jobParams.InputHeight.HasValue && jobParams.InputHeight != data.InputHeight)
                    data.InputHeight = jobParams.InputHeight;
                if (jobParams.InputId.IsPositive() && jobParams.InputId != data.InputId)
                    data.InputId = jobParams.InputId;
                if (jobParams.InputWidth.HasValue && jobParams.InputWidth != data.InputWidth)
                    data.InputWidth = jobParams.InputWidth;

                if (data.UploadStarted.HasValue && data.JobCompleted.HasValue && !data.JobDuration.HasValue)
                {
                    data.JobDuration = (data.JobCompleted.Value - data.UploadStarted.Value).TotalSeconds;
                }

                db.SubmitChanges();
                return data;
            }
        }

        //started doing this but got frustrated with deferred requests to email noticiations
        //public static object ProcessEncodeResult(EncodeJobParams result)
        //{
        //    int videoId = result.InputId;
        //    //first thing we do is check to make sure the video hasn't already been marked as completed or screwed up somehow.  this happened a bit in the past, and ideally wouldn't happen in a perfect world
        //    using (var db = DataModel.ReadOnly)
        //    {
        //        var upload = (from tj in db.EncodeLogs where tj.InputId == result.InputId select tj).SingleOrDefault();
        //        if (upload == null || upload.JobCompleted.HasValue)
        //        {
        //            return null;
        //        }
        //    }

        //    var error = result.ErrorCode != TranscodeJobErrorCodes.NotSet;

        //    var video = Things.GetOrNull<VideoThing>(videoId);
        //    ProjectThing production = video == null ? null : video.FindAncestor<ProjectThing>();
        //    FileThing videoFile = video == null ? null : video.FindChild<FileThing>();

        //    //if production or video is null, they deleted it before the transcoding completed.  in this case we just kill the file and go on
        //    if (video == null || error)
        //    {
        //        if (result.Outputs != null && result.Outputs.Length > 0)
        //        {
        //            result.Outputs.Each(target =>
        //            {
        //                if (target.Url.HasChars())
        //                {
        //                    S3.TryDeleteFile(target.Url);
        //                }
        //            });
        //        }
        //        if (result.Thumbnails != null && result.Thumbnails.Length > 0)
        //        {
        //            result.Thumbnails.Each(target =>
        //            {
        //                if (target.Url.HasChars())
        //                {
        //                    S3.TryDeleteFile(target.Url);
        //                }
        //            });
        //        }

        //        if (video != null)
        //        {
        //            this.DeferRequest<EmailController>(c => c.SendProductionVideoTranscodeError(video.CreatedByUserId, production.Id, videoFile.OriginalFileName, result.ErrorCode));
        //            video.TrackChanges();
        //            video.IsComplimentary = true;//this is necessary to ensure Account.VideosAddedInBillingCycle is right
        //            video.Update();
        //            video.Delete();
        //        }
        //    }
        //    else
        //    {
        //        Debug.Assert(result.EncodingCompleted.HasValue);

        //        using (var insertBatcher = new CommandBatcher())
        //        {
        //            var now = DateTime.UtcNow;
        //            foreach (var outputVideo in result.Outputs)
        //            {
        //                var videoStream = video.AddChild(new VideoStreamThing
        //                {
        //                    CreatedByUserId = video.CreatedByUserId,
        //                    CreatedOn = now,
        //                    VideoBitRate = outputVideo.ActualVideoBitRate,
        //                    AudioBitRate = outputVideo.ActualAudioBitRate,
        //                    DeletePhysicalFile = true,
        //                    Profile = outputVideo.Profile,
        //                });

        //                videoStream.Url = outputVideo.Url;
        //                videoStream.Bytes = outputVideo.FileSize;
        //                videoStream.QueueInsertData(insertBatcher);
        //            }

        //            if (result.Thumbnails != null)
        //            {
        //                foreach (var thumbnail in result.Thumbnails)
        //                {
        //                    video.AddChild(new VideoThumbnailThing
        //                    {
        //                        CreatedByUserId = video.CreatedByUserId,
        //                        CreatedOn = now,
        //                        Url = thumbnail.Url,
        //                        Time = thumbnail.Time,
        //                        Width = thumbnail.Width,
        //                        Height = thumbnail.Height,
        //                        DeletePhysicalFile = true,
        //                        LogInsertActivity = false,
        //                    }).QueueInsertData(insertBatcher);
        //                }
        //            }
        //            insertBatcher.Execute();

        //            //could maybe have saved a db command by putting UpdateEncodeLog before the update, but it's only one small db hit and the job isn't done quite yet
        //            using (var db = DataModel.ReadOnly)
        //            {
        //                (from t in db.EncodeLogs where t.InputId == result.InputId select t.InputDuration).Single().IfHasVal(v => video.Duration = TimeSpan.FromSeconds(v));
        //            }
        //            var changes = new Dictionary<string, object>();
        //            changes["HasVideo"] = video.HasVideo;
        //            changes["Thumbnails"] = video.Children.OfType<VideoThumbnailThing>().OrderBy(t => t.Time).Select(c => (VideoThumbnailThingView)c.CreateViewData(this.Identity())).ToArray();
        //            video.Update(true, null, changes);
        //        }

        //        this.DeferRequest<EmailController>(c => c.NotifyForNewVideo(video.Id));
        //    }

        //    result.JobCompleted = DateTime.UtcNow;
        //    Videos.UpdateEncodeLog(result);

        //    return null;
        //}

    }
}
