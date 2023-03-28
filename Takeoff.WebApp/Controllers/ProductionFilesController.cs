using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.S3.Model;
using Mediascend.Web;
using Takeoff.Models;
using Takeoff.Data;
using MvcContrib.ActionResults;
using Takeoff.Controllers;
using MvcContrib;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.IO;


namespace Takeoff.Controllers
{
    public class ProductionFilesController : BasicController
    {

        /// <summary>
        /// Allocates an upload of 1-x files into a production.
        /// </summary>
        /// <param name="productionId"></param>
        /// <param name="filesToUpload"></param>
        /// <returns></returns>
        [AuthorizeProductionAccess(ProductionIdParam = "productionId")]
        public ActionResult Allocate(int productionId, FileToUpload fileToUpload)
        {
            Args.ValidKey(productionId, "productionId");
            Args.NotNull(fileToUpload, "fileToUpload");

            var production = Things.Get<ProjectThing>(productionId);
            production.VerifyCreate(typeof(FileThing), this.UserThing());

            var fileSize = fileToUpload.bytes;
            var user = this.UserThing();

            var result = CheckLimits(production, user, new FileSize(fileSize));
            if (result != null)
                return result;

            fileToUpload.PrepareForUpload(new S3FileLocation { Location = production.FilesLocation }, FileAccess.Private, true, fileSize);
            return Json(fileToUpload);
        }

        /// <summary>
        /// POST: /ProductionFiles
        /// 
        /// Called by the client after a successful upload, this adds the necessary database records for the new file(s).
        /// </summary>
        [AuthorizeProductionAccess(ProductionIdParam = "productionId")]
        public ActionResult Create(int productionId, string fileKey, string fileName)
        {
            Args.ValidKey(productionId, "productionId");
            Args.HasChars(fileKey, "fileKey");

            fileName = fileName.CharsOr(fileKey);
            var production = Things.Get<ProjectThing>(productionId);
            production.VerifyCreate(typeof(FileThing), this.UserThing());

            var user = this.UserThing();
            var account = user.Account;

            var uploaded = new S3FileLocation
            {
                Bucket = ConfigUtil.GetRequiredAppSetting("UploadBucket"),
                Key = fileKey
            };

            using (var s3Client = new Amazon.S3.AmazonS3Client(Aws.AccessKey, Aws.SecretKey))
            {
                var data = s3Client.GetObjectMetadata(new GetObjectMetadataRequest
                                                          {
                                                              BucketName = uploaded.Bucket,
                                                              Key = uploaded.Key
                                                          });
                var fileSize = (int)data.ContentLength;
                var result = CheckLimits(production, user, new FileSize(fileSize));//shouldn't happen unless input is tampered with
                if (result != null)
                    return result;

                var location = new S3FileLocation
                {
                    Bucket = ConfigUtil.GetRequiredAppSetting("ProductionBucket")
                };
                var extension = uploaded.FileName.HasChars() ? System.IO.Path.GetExtension(uploaded.FileName) : string.Empty;
                var key = production.Id.ToInvariant() + "/" + Guid.NewGuid().StripDashes();
                if (extension.HasChars())
                    key = key + extension.StartWith(".");
                location.Key = key;
                //move to production bucket
                s3Client.CopyObject(new CopyObjectRequest
                {
                    CannedACL = Amazon.S3.S3CannedACL.Private,
                    SourceBucket = uploaded.Bucket,
                    SourceKey = uploaded.Key,
                    DestinationBucket = location.Bucket,
                    DestinationKey = location.Key,
                });
                s3Client.DeleteObject(new DeleteObjectRequest
                {
                    BucketName = uploaded.Bucket,
                    Key = uploaded.Key,
                });


                var file = new FileThing
                {
                    CreatedByUserId = this.UserThing().Id,
                    CreatedOn = DateTime.UtcNow,
                    OriginalFileName = fileName,
                    Location = location.Location,
                    FileName = location.FileName,
                    Bytes = fileSize,
                    DeletePhysicalFile = true,
                };
                production.AddChild(file);

                using (var insertBatcher = new CommandBatcher())
                {
                    //hook into the CommandBatcher to add an upload record for the file.  this could have been done using a Delay or something but this cuts down on a request
                    insertBatcher.Executing += (o, e) =>
                    {
                        insertBatcher.QueueInsertAutoId(new Models.Data.FileUploadLog
                        {
                            AccountId = production.AccountId,
                            Bytes = fileSize,
                            Date = this.RequestDate(),
                            FileThingType = file.Type,
                            Url = location.Url,
                            OriginalFileName = fileName,
                            UserId = this.UserId(),
                            UploadCompleted = this.RequestDate(),
                        }, new SubstituteParameter[]{ new SubstituteParameter
                    {
                        ColumnName = "FileThingId",
                        ValueParameter = file.InsertIdParam
                    }});
                    };
                    file.QueueInsertData(insertBatcher);
                    insertBatcher.Execute();
                    file.RemoveFromCache();
                }

                this.DeferRequest<EmailController>(c => c.NotifyForNewProjectFile(file.Id));

                return Json(new
                {
                    Data = file.CreateViewData(this.Identity()),
                    ActivityPanelItem = file.GetActivityPanelContents(new Models.Data.ActionSource { Action = "Add" }, true, this.Identity()),
                });
            }
        }

        /// <summary>
        /// Checks whether the user can upload the file for the production.  If not, they will be redirected to a page that gives them more info.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="user"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        /// <remarks>Videos are added only via ajax now so only json objects are returned right now.  </remarks>
        private ActionResult CheckLimits(ProjectThing project, UserThing user, FileSize fileSize)
        {
            AccountThing account = Things.GetOrNull<AccountThing>(project.AccountId);
            if ( account == null )
                return null;//something funky
            Func<bool> isUsersAccount = () =>
                {
                    var userAccount = user.Account;
                    return userAccount != null && account.Id == userAccount.Id;
                };

            var plan = account.Plan;
            if (plan.AssetFileMaxSize.HasValue && plan.AssetFileMaxSize.Value.Bytes > 0 && plan.AssetFileMaxSize.Value <= fileSize)
            {
                return this.RedirectToAction<AccountController>(c => c.LimitReached("assetFileSize", isUsersAccount()));
            }
            if (plan.AssetsTotalMaxCount.HasValue && plan.AssetsTotalMaxCount.Value <= account.AssetFilesCount)
            {
                return this.RedirectToAction<AccountController>(c => c.LimitReached("assetCount", isUsersAccount()));
            }
            if (plan.AssetsTotalMaxSize.HasValue && plan.AssetsTotalMaxSize.Value.Bytes > 0 && plan.AssetsTotalMaxSize.Value <= (account.AssetsTotalSizeBillable + fileSize))
            {
                return this.RedirectToAction<AccountController>(c => c.LimitReached("assetTotalSize", isUsersAccount()));
            }
            if (plan.AssetsAllTimeMaxCount.HasValue && plan.AssetsAllTimeMaxCount.Value <= account.AssetFilesAllTimeCount)
            {
                return this.RedirectToAction<AccountController>(c => c.LimitReached("assetAllTimeCount", isUsersAccount()));
            }
            return null;
        }


        /// <summary>
        /// Called by js when user deletes a file.  Didn't use Destroy action because this is not done using a form.
        /// </summary>
        /// <param name="productionFileId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        public ActionResult Delete(int id)
        {
            Things.Get<FileThing>(id).Verify(Permissions.Delete, this.UserThing()).Delete();
            return this.Empty();
        }

        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        public ActionResult Download(int id)
        {
            var file = Things.Get<FileThing>(id);
            //prevent downloading of video file sources or any other type of file.  
            var video = file.FindAncestor<VideoThing>();
            if (video != null)
                throw new ArgumentException("Thing wasn't a valid asset file.");
            var production = file.FindAncestor<ProjectThing>();//no need to verify because AuthorizeProductionAccess handles it
            
            var location = new S3FileLocation { Location = file.Location, FileName = file.FileName };
            this.Defer(() => ActivityLogs.LogDownload(this.UserId(), this.RequestDate(), file.AccountId, file.Id, file.Type, location.Url, file.Bytes.GetValueOrDefault(), file.OriginalFileName, !file.DeletePhysicalFile));


            using (var s3 = new Amazon.S3.AmazonS3Client(Aws.AccessKey, Aws.SecretKey))
            {
                var filename = file.OriginalFileName.CharsOr("file");
                var url = s3.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    ResponseHeaderOverrides = new ResponseHeaderOverrides
                    {
                        ContentDisposition = "attachment; filename=\"" + FileToUpload.CleanFileName(filename) + "\""
                    },
                    BucketName = location.Bucket,
                    Key = location.Key,
                    Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(120))//expires in 120 minutes...which I suppose is a good amount of time to account for any differences especially during daylight savings transition: todo: lower this once you deploy to amazon ec2 
                });
                return Redirect(url);
            }
        }

    }
}
