using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Takeoff.Models.Data;
using System.Configuration;
using Amazon.S3;

namespace Takeoff.Models
{
    public static class ServerRequestLogging
    {

        public static void SaveRequestDetails(Guid requestId, string report, string contentType)
        {
            var key = "{0}-Request".FormatString(requestId);
            var uploadBucket = ConfigurationManager.AppSettings["WebRequestLogBucket"];
            using (var s3 = new Amazon.S3.AmazonS3Client(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]))
            {
                s3.PutObject(new PutObjectRequest
                {
                    BucketName = uploadBucket,
                    CannedACL = S3CannedACL.AuthenticatedRead,
                    Key = key,
                    ContentBody = report,
                    ContentType = contentType
                });
            }
        }

        public static void SaveResponseDetails(Guid requestId, string report, string contentType)
        {
            var key = "{0}-Response".FormatString(requestId);
            var uploadBucket = ConfigurationManager.AppSettings["WebRequestLogBucket"];
            using (var s3 = new Amazon.S3.AmazonS3Client(ConfigurationManager.AppSettings["AmazonAccessKey"], ConfigurationManager.AppSettings["AmazonSecretKey"]))
            {
                s3.PutObject(new PutObjectRequest
                {
                    BucketName = uploadBucket,
                    CannedACL = S3CannedACL.AuthenticatedRead,
                    Key = key,
                    ContentType = contentType,
                    ContentBody = report,
                });
            }
        }

        public static void SaveRequestToDb(Guid requestId, DateTime requestStartedOn, bool savingRequest, bool savingResponse, int? userId, string urlLocalPath)
        {
            using ( var db = new DataModel())
            {
                db.ServerRequestLogs.InsertOnSubmit(new ServerRequestLog
                                                     {
                                                         Id = requestId,
                                                         RequestedOn = requestStartedOn,
                                                         SavedRequest = savingRequest,
                                                         SavedResponse = savingResponse,
                                                         UserId = userId,
                                                         UrlLocalPath = urlLocalPath,
                                                     });
                db.SubmitChanges();
            }
        }
    }

}
