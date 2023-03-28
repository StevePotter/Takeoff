using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using CommandLine;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{
       
    public class DeleteS3FilesCommandOptions
    {
        [Option('p', "path", Required = true, HelpText = "Path to the expored csv. ")]
        public string CsvPath { get; set; }

    }

    /// <summary>
    /// This is a temporary command created for a huge delete after deleting beta accts.
    /// </summary>
    public class DeleteS3FilesCommand : BaseCommandWithOptions<DeleteS3FilesCommandOptions>
    {
        public DeleteS3FilesCommand()
        {
            EnableXmlReport = true;
            NotifyOnErrors = true;
            LogJobInDatabase = false;
        }


        protected override void Perform(DeleteS3FilesCommandOptions arguments)
        {
            var accessKey = ConfigUtil.GetRequiredAppSetting("AmazonAccessKey");
            var secretKey = ConfigUtil.GetRequiredAppSetting("AmazonSecretKey");
            var awsS3Source = new AmazonS3Client(accessKey, secretKey);
            var litS3Source = new LitS3.S3Service
            {
                AccessKeyID = accessKey,
                SecretAccessKey = secretKey,
            };
            var files = System.IO.File.ReadAllLines(arguments.CsvPath).Where(p => p.HasChars()).Select(p => new S3FileLocation
                                                                                                           {
                                                                                                                Location = p.Split(',')[0].Trim(),
                                                                                                                FileName = p.Split(',')[1].Trim(),
                                                                                                           }).ToArray();
            foreach( var file in files)
            {
                Step("DeleteFile", true, () =>
                                             {
                                                 AddReportAttribute("Bucket", file.Bucket);
                                                 AddReportAttribute("Key", file.Key);
                                                 bool delete = false;
                                                 try
                                                 {
                                                     var metaData =
                                                         awsS3Source.GetObjectMetadata(new GetObjectMetadataRequest
                                                                                           {
                                                                                               BucketName = file.Bucket,
                                                                                               Key = file.Key
                                                                                           });
                                                     //key existed.  delete it
                                                     AddReportAttribute("Existed", true);
                                                     delete = true;

                                                 }
                                                 catch (Amazon.S3.AmazonS3Exception ex)
                                                 {
                                                     if (ex.StatusCode != HttpStatusCode.NotFound)
                                                     {
                                                         throw;
                                                     }
                                                 }
                                                 if (delete)
                                                 {
                                                     awsS3Source.DeleteObject(new DeleteObjectRequest
                                                                                  {
                                                                                      BucketName = file.Bucket,
                                                                                      Key = file.Key,
                                                                                  });
                                                 }
                                             });
            }
       

        }

    }
}

