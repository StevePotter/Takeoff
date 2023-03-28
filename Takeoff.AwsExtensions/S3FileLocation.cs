using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3.Model;
using System.IO;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Collections;
using Amazon.S3;

namespace Takeoff
{


    /// <summary>
    /// Provides ability to translate from typical folder/filename structure to Amazon's bucket/key system.
    /// </summary>
    public class S3FileLocation
    {
        public S3FileLocation()
        {

        }

        /// <summary>
        /// The name of the S3 bucket.
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// The path to the folder, which is the bucket name combined with the FolderInBucket.
        /// </summary>
        public string Location
        {
            get
            {
                if (_Location != null)
                    return _Location;
                if (Bucket != null)
                    return FolderInBucket == null ? Bucket : Bucket.EndWith("/") + FolderInBucket.StartWithout("/");
                return null;
            }
            set
            {
                _Location = value;
                var split = value.IndexOf('/');
                if (split < 0)
                    Bucket = value;
                else
                {
                    Bucket = value.Substring(0, split);
                    FolderInBucket = value.Substring(split + 1);
                }
            }
        }
        private string _Location;

        public string FileName { get; set; }

        /// <summary>
        /// The folder(s) within the bucket for the given file.  This is actually kept in the Key.
        /// </summary>
        public string FolderInBucket { get; private set; }


        public string Key
        {
            get
            {
                if (_Key != null)
                    return _Key;
                if (FileName == null)
                    return null;
                if (string.IsNullOrEmpty(FolderInBucket))
                    return FileName;
                else
                    return FolderInBucket.EndWith("/") + FileName;
            }
            set
            {
                _Key = value;

                var split = value.LastIndexOf('/');
                if (split >= 0)
                {
                    this.FolderInBucket = value.Substring(0, split);
                    this.FileName = value.Substring(split + 1);
                }
                else
                {
                    FileName = value;
                }

            }
        }
        private string _Key;


        public string BucketUrl(Protocol protocol)
        {
            return (protocol == Protocol.HTTP ? "http://" : "https://") + Bucket + ".s3.amazonaws.com/";
        }

        /// <summary>
        /// Gets a http url to the s3 file.  Note that if the file is not public readable, the url won't work.  It will need to the authorized.
        /// </summary>
        public string Url
        {
            get
            {
                return GetUrl();
            }
        }

        /// <summary>
        /// Gets the url to an s3 file.  The file must be readable to the public.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public string GetUrl(Protocol protocol = Protocol.HTTP)
        {
            return BucketUrl(protocol).EndWith("/") + Key.StartWithout("/");
        }

        /// <summary>
        /// Takes a constructed url like http://to-d-projects.s3.amazonaws.com/17933/32fa46ff35d24a149146a9c07a41d4a9_0001.png and turns it into an s3filelocation object.
        /// Also can handle an S3 url like: s3://bucket-name/path/filename
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static S3FileLocation FromUrl(string url)
        {
            string bucket;
            if (url.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
            {
                url = url.StartWithout("s3://", StringComparison.OrdinalIgnoreCase);
                bucket = url.Before("/");
                url = url.After("/");                
            }
            else
            {
                url = url.StartWithout("http://", StringComparison.OrdinalIgnoreCase).StartWithout("https://", StringComparison.OrdinalIgnoreCase);
                bucket = url.Before(".s3.amazonaws.com", StringComparison.OrdinalIgnoreCase);
                url = url.After("amazonaws.com/");
            }
            //you can also sometimes put the bucket as the first subfolder like s3.amazonaws.com/bucket/xxxxx
            if (!bucket.HasChars())
            {
                bucket = url.Before("/");
                url = url.After("/");
            }

            string key = url.Contains("?") ? url.Before("?") : url.StartWithout("/");
            return new S3FileLocation
            {
                Bucket = bucket,
                Key = key
            };
        }

        public string GetAuthorizedUrl(TimeSpan expires, Amazon.S3.Protocol protocol = Amazon.S3.Protocol.HTTP)
        {
            return GetAuthorizedUrl(DateTime.UtcNow.Add(expires), protocol);
        }


        public string GetAuthorizedUrl(DateTime expires, Amazon.S3.Protocol protocol = Amazon.S3.Protocol.HTTP)
        {
            using (var client = S3.CreateAmazonClient())
            {
                return client.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest
                {
                    BucketName = Bucket,
                    Key = Key,
                    Protocol = protocol,
                    Verb = Amazon.S3.HttpVerb.GET,
                    Expires = expires
                });
            }
        }

    }

    public static class Aws
    {
        public static string SecretKey
        {
            get
            {
                return _secretKey.Value;
            }
        }
        private static Lazy<string> _secretKey = new Lazy<string>(() => ConfigurationManager.AppSettings["AmazonSecretKey"]);

        public static string AccessKey
        {
            get
            {
                return _AccessKey.Value;
            }
        }
        private static Lazy<string> _AccessKey = new Lazy<string>(() => ConfigurationManager.AppSettings["AmazonAccessKey"]);


    }

    /// <summary>
    /// Utility functions for working with Amazon S3 in our apps. 
    /// </summary>
    public static class S3
    {


        /// <summary>
        /// Creates a signature that can be sent to S3 for an upload POST.
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static string CreateSignature(string policy)
        {
            return policy.Hash(Aws.SecretKey, "HMACSHA1");
        }


        /// <summary>
        /// Creates the base 64 string for the policy object for the given POST upload.
        /// http://docs.amazonwebservices.com/AmazonS3/2006-03-01/
        /// </summary>
        /// <returns></returns>
        public static string CreatePolicy(string fileKey, string bucketName, FileAccess access, DateTime? expiration, int? bytes, Dictionary<string, string> extraHeaders, bool addStartsWith = true)
        {
            var policyData = new Dictionary<string, object>();
            if (expiration.HasValue)
                policyData.Add("expiration", expiration.Value.ToString("yyyy-MM-ddTHH:mm:ss.000Z"));

            var conditions = new ArrayList();
            policyData.Add("conditions", conditions);

            //quick util method
            Action<string, object> addCondition = (cName, cValue) =>
            {
                var conditionData = new Dictionary<string, object>();
                conditionData.Add(cName, cValue);
                conditions.Add(conditionData);
            };

            addCondition("acl", ConvertFileAccessToPolicyString(access));
            addCondition("bucket", bucketName);
            addCondition("key", fileKey);
            if (bytes.HasValue)
            {
                conditions.Add(new object[] { "content-length-range", bytes.Value.ToInvariant(), (bytes.Value).ToInvariant() });
            }
            if (extraHeaders != null && extraHeaders.Count > 0)
            {
                foreach (var keyVal in extraHeaders)
                    addCondition(keyVal.Key, keyVal.Value);
            }
            if (addStartsWith)
                conditions.Add(new object[] { "starts-with", "$Filename", "" });//this line is critical for Flash uploads to S3.  Excluding it will break the upload.

            string json = new JavaScriptSerializer().Serialize(policyData);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        /// <summary>
        /// Takes the FileAccess enum value and converts to the string recognized by the S3 API.
        /// </summary>
        /// <param name="access"></param>
        /// <returns></returns>
        public static string ConvertFileAccessToPolicyString(FileAccess access)
        {
            switch (access)
            {
                case FileAccess.Private:
                    return "private";
                case FileAccess.PublicRead:
                    return "public-read";
                default:
                    return "public-read-write";
            }

        }

        public static void DeleteFile(S3FileLocation location)
        {
            new LitS3.S3Service
            {
                AccessKeyID = Aws.AccessKey,
                SecretAccessKey = Aws.SecretKey
            }.DeleteObject(location.Bucket, location.Key);
        }


        public static void DeleteFile(string location, string fileName)
        {
            DeleteFile(new S3FileLocation { Location = location, FileName = fileName });
        }


        public static bool TryDeleteFile(string location, string fileName)
        {
            return TryDeleteFile(new S3FileLocation { Location = location, FileName = fileName });
        }

        public static bool TryDeleteFile(S3FileLocation location)
        {
            try
            {
                DeleteFile(location);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryDeleteFile(string url)
        {
            try
            {
                DeleteFile(S3FileLocation.FromUrl(url));
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static Amazon.S3.AmazonS3Client CreateAmazonClient()
        {
            return new Amazon.S3.AmazonS3Client(Aws.AccessKey, Aws.SecretKey);
        }



        /// <summary>
        /// Uploads a file that is cached in browser for a long long time and is public read
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <param name="isGZipped"></param>
        public static void UploadFileToS3AsPublicCached(MemoryStream inputStream, string bucketName, string key, bool isGZipped)
        {
            UploadFileToS3(inputStream, bucketName, key, "public, max-age=31536000", LitS3.CannedAcl.PublicRead, isGZipped);//use max-age instead of expires because its a sliding expiration
        }

        public static void UploadFileToS3(MemoryStream inputStream, string bucketName, string key, string cacheControl, LitS3.CannedAcl acl, bool isGZipped)
        {
            var s3 = new LitS3.S3Service { SecretAccessKey = Aws.SecretKey, AccessKeyID = Aws.AccessKey };

            var request = new LitS3.AddObjectRequest(s3, bucketName, key);
            request.CannedAcl = acl;
            request.ContentLength = inputStream.Length;
            if (cacheControl.HasChars())
                request.CacheControl = cacheControl;
            if (isGZipped)
                request.ContentEncoding = "gzip";
            request.ContentType = FileUtil.GetMimeType(System.IO.Path.GetExtension(key));
            inputStream.Position = 0;
            request.PerformWithRequestStream(delegate(Stream stream)
            {
                inputStream.CopyTo(stream);
                stream.Flush();
            });
        }



    }

    /// <summary>
    /// Indicates the access level for a file in an S3 bucket.
    /// </summary>
    public enum FileAccess
    {
        Private,
        PublicRead,
        PublicReadWrite,
        //        AuthenticatedRead  NEVER USE AUTHENTICATED-READ!  It turns out that it's ANY S3 user that's authenticated, which means anyone can view it if they have an Amazon account.
    }


}
