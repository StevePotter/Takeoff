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
       
    public class FileCleanupCommandOptions
    {
        [Option('d', "days", Required = true, HelpText = "The number of days that must pass after a file is deleted from the app before it is physically deleted.")]
        public int DaysToRetainPhysicalFiles { get; set; }

    }

    public class FileCleanupCommand : BaseCommandWithOptions<FileCleanupCommandOptions>
    {
        public FileCleanupCommand()
        {
            EnableXmlReport = true;
            NotifyOnErrors = true;
            LogJobInDatabase = true;
        }


        protected override void Perform(FileCleanupCommandOptions arguments)
        {
            if ( arguments.DaysToRetainPhysicalFiles < 7)
                throw new Exception("Woah there partner.  You should never delete files less than a week removed from the system.");
            var oldestAge = DateTime.UtcNow.Subtract(TimeSpan.FromDays(arguments.DaysToRetainPhysicalFiles));
            var totalSizeDeleted = new FileSize();
            Takeoff.Models.Data.File[] files = null;
            var accessKey = ConfigUtil.GetRequiredAppSetting("AmazonAccessKey");
            var secretKey = ConfigUtil.GetRequiredAppSetting("AmazonSecretKey");
            var awsS3Source = new AmazonS3Client(accessKey, secretKey);
            var litS3Source = new LitS3.S3Service
            {
                AccessKeyID = accessKey,
                SecretAccessKey = secretKey,
            };

            using (var db = new DataModel())
            {
                Step("GetFilesToDelete", () =>
                                             {
                                                 files = (from ft in db.Things
                                                          join f in db.Files on ft.Id equals f.ThingId
                                                          where
                                                              f.DeletePhysicalFile && ft.DeletedOn != null &&
                                                              ft.DeletedOn.Value < oldestAge &&
                                                              (f.PhysicalFileDeleted == null ||
                                                               !f.PhysicalFileDeleted.Value)
                                                          select f).ToArray();
                                                 AddReportAttribute("FileCount", files.Count());
                                             });
                if (files.HasItems())
                {
                    foreach (var file in files)
                    {
                        Step("DeleteFile", true, () =>
                                               {
                                                   var size = new FileSize(file.Bytes.GetValueOrDefault());
                                                   AddReportAttribute("FileSize",size);
                                                   var location = new S3FileLocation { Location = file.Location, FileName = file.FileName };

                                                   bool delete = false;
                                                   try
                                                   {
                                                       var metaData = awsS3Source.GetObjectMetadata(new GetObjectMetadataRequest
                                                                                                         {
                                                                                                             BucketName = location.Bucket,
                                                                                                             Key = location.Key
                                                                                                         });
                                                       //key existed.  delete it
                                                       delete = true;
                                                   }
                                                   catch (Amazon.S3.AmazonS3Exception ex)
                                                   {
                                                       if (ex.StatusCode == HttpStatusCode.NotFound)
                                                       {
                                                           file.PhysicalFileDeleted = true;//assume the file was previously deleted.  update the database so this doesn't keep happening
                                                           db.SubmitChanges();
                                                           totalSizeDeleted += size;
                                                       }
                                                       else
                                                       {
                                                           throw;
                                                       }
                                                   }
                                                   if (delete)
                                                   {
                                                       awsS3Source.DeleteObject(new DeleteObjectRequest
                                                                                    {
                                                                                        BucketName = location.Bucket,
                                                                                        Key = location.Key,
                                                                                    });
                                                       file.PhysicalFileDeleted = true;//assume the file was previously deleted.  update the database so this doesn't keep happening
                                                       db.SubmitChanges();
                                                       totalSizeDeleted += size;
                                                   }
                                               });
                    }
                }
                AddReportAttribute("TotalSize", totalSizeDeleted.ToString());
            }

        }

    }
}

