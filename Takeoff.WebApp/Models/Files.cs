using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Amazon.S3.Model;
using Takeoff;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using Amazon.S3;

namespace Mediascend.Web
{


    /// <summary>
    /// Passed back and forth between the server and the madUpload component.  Contains all the information necessary to do an authenticated upload of a new file to S3.
    /// </summary>
    public class FileToUpload
    {
        /// <summary>
        /// The index of this file within its client-side list.  Sent back to the upload control for synchroniziation.
        /// </summary>
        public int index { get; set; }
        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public int bytes { get; set; }
        /// <summary>
        /// The file name selected by the user.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The bucket name that is being uploaded to.
        /// </summary>
        public string bucketName { get; set; }

        /// <summary>
        /// The final name of the file in S3 (also known as the "key")
        /// </summary>
        public string fileKey { get; set; }

        /// <summary>
        /// The url to post the file to.
        /// </summary>
        public string targetUrl { get; set; }

        /// <summary>
        /// A Takeoff signature that lets us verify that nobody tampered with the important fields like fileKey, bucketName, and bytes.  Has nothing to do with Amazon.
        /// </summary>
        public string signature { get; set; }


        /// <summary>
        /// The form variables that contain the policy and other goodies that S3 needs.
        /// </summary>
        [XmlIgnore()]//needed for web services because asp.net threw an exception when figuring out the schema.  this means you can't use FileToUpload methods via SOAP, but you can use JSON.
        public Dictionary<string, string> variables { get; private set; }

        public static string FileUploadSignatureKey = ConfigUtil.AppSetting("FileUploadSignatureKey");


        public FileToUpload()
        {
            variables = new Dictionary<string, string>();
        }

        /// <summary>
        /// Used to prevent input tampering.
        /// </summary>
        /// <returns></returns>
        public string GenerateSignature()
        {
            return string.Concat(fileKey.CharsOrEmpty(), bucketName.CharsOrEmpty(), this.bytes.ToInvariant()).Hash(FileUploadSignatureKey, "HMACSHA1");
        }


        /// <summary>
        /// Sets the necessary properties needed to upload via POST to S3.  Generates a unique file key, sets signatures and POST variables 
        /// so an authenticated S3 upload can be made.  
        /// The name and size properties should already be set.
        /// This will be converted to JSON data and given back to the client-side control, so it must return JSON-convertible types.
        /// Each object returned will be included in the form post data.
        /// See http://docs.amazonwebservices.com/AmazonS3/2006-03-01/ for more info
        /// </summary>
        /// <param name="filesData"></param>
        /// <param name="bucketName"></param>
        /// <param name="fileAccess">
        /// <param name="isAttachment">Set to true when the file is meant to be downloaded and saved using the original file name.  Set to false for streaming and html files.</param>
        /// <param name="fileSize">The size, from the upload plugin that the file is.  We embed this in the policy document.  This way nobody can tamper with form input and upload a file that is actually 1gb but we think it's 1mb. </param>
        /// </param>
        /// <returns></returns>
        public void PrepareForUpload(S3FileLocation location, Takeoff.FileAccess fileAccess, bool isAttachment, int fileSize)
        {
            if (!this.bytes.IsPositive())
                throw new InvalidOperationException("bytes must be set");
            if (string.IsNullOrEmpty(name))
                throw new InvalidOperationException("name must be set");

            bucketName = location.Bucket;
            targetUrl = location.BucketUrl(Protocol.HTTP);

            var safeFileName = CleanFileName(name);//have to do this because other OSs allow illegal chars.  For example, mac allows double quotes.  Without this, getextension would throw an exception and content-disposition header could be wrong
            var extension = System.IO.Path.GetExtension(safeFileName);
            location.FileName = Guid.NewGuid().StripDashes() + (string.IsNullOrEmpty(extension) ? String.Empty : extension.StartWith("."));//use a guid to guarantee a unique file name

            fileKey = location.Key;

            var extraHeaders = new Dictionary<string, string>();
            if (isAttachment)
                extraHeaders.Add("Content-Disposition", "attachment; filename=\"" + safeFileName + "\"");
            if (extension.HasChars())
                extraHeaders.Add("Content-Type", FileUtil.GetMimeType(extension));
            extraHeaders.Add("success_action_status", "201");//this line is crucial to fix a Mac flash upload bug where uploadcomplete never fires: http://swfupload.org/forum/generaldiscussion/465.  
            string policy = S3.CreatePolicy(fileKey, location.Bucket, fileAccess, DateTime.UtcNow.AddMinutes(86400), fileSize, extraHeaders);//give em a day to finish uploading

            variables["key"] = fileKey;
            foreach (var keyVal in extraHeaders)
                variables[keyVal.Key] = keyVal.Value;
            variables["policy"] = policy;
            variables["signature"] = S3.CreateSignature(policy);
            variables["AWSAccessKeyId"] = Aws.AccessKey;
            variables["acl"] = S3.ConvertFileAccessToPolicyString(fileAccess);

            signature = GenerateSignature();
   
        }


        private static string fileNameCleanerExpression = "[" + string.Join("", Array.ConvertAll(System.IO.Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))) + "]";
        private static Regex fileNameCleaner = new Regex(fileNameCleanerExpression, RegexOptions.Compiled);
//http://stackoverflow.com/questions/146134/how-to-remove-illegal-characters-from-path-and-filenames

        /// <summary>
        /// Removes all illegal characters from a file name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CleanFileName(string fileName)
        {
            return fileNameCleaner.Replace(fileName, "");
        }

    }

}
