using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takeoff.Models
{
    /// <summary>
    /// Some basic logging during regular app activity. 
    /// </summary>
    public static class ActivityLogs
    {

        public static void LogDownload(int userId, System.DateTime date, int accountId, int fileThingId, string fileThingType, string fileUrl, int bytes, string originalFileName, bool isSampleFile)
        {
            using (var db = new DataModel())
            {
                db.FileDownloadLogs.InsertOnSubmit(new Models.Data.FileDownloadLog
                {
                    AccountId = accountId,
                    Bytes = bytes,
                    Date = date,
                    Url = fileUrl,//this should not include authorization params or anything...just the file path
                    FileThingId = fileThingId,
                    FileThingType = fileThingType,
                    IsSampleFile = isSampleFile,
                    OriginalFileName = originalFileName,
                    UserId = userId,
                });
                db.SubmitChanges();
            }
        }

        public static void LogUpload(int userId, System.DateTime date, int accountId, int fileThingId, string fileThingType, string fileUrl, int bytes, string originalFileName)
        {
            using (var db = new DataModel())
            {
                db.FileUploadLogs.InsertOnSubmit(new Models.Data.FileUploadLog
                {
                    AccountId = accountId,
                    Bytes = bytes,
                    Date = date,
                    Url = fileUrl,//this should not include authorization params or anything...just the file path
                    FileThingId = fileThingId,
                    FileThingType = fileThingType,
                    OriginalFileName = originalFileName,
                    UserId = userId,
                });
                db.SubmitChanges();
            }
        }


        public static void LogThingAccess(int userId, System.DateTime date, int thingId)
        {
            using (var db = new DataModel())
            {
                db.ThingAccessLogs.InsertOnSubmit(new Models.Data.ThingAccessLog
                {
                    Date = date,
                    ThingId = thingId,
                    UserId = userId,
                });
                db.SubmitChanges();
            }
        }

        public static void LogWatch(System.DateTime date, int userId, int accountId, int productionId, int videoId, int videoStreamId, string url, bool isSample, int bytes, string profile, System.Nullable<double> duration)
        {
            using (var db = new DataModel())
            {
                db.VideoWatchLogs.InsertOnSubmit(new Models.Data.VideoWatchLog
                {
                    AccountId = accountId,
                    Bytes = bytes,
                    Date = date,
                    Duration = duration,
                    IsSample = isSample,
                    Profile = profile,
                    ProductionId = productionId,
                    VideoId = videoId,
                    VideoStreamId = videoStreamId,
                    Url = url,
                    UserId = userId
                });
                db.SubmitChanges();
            }
        }
    }
}
