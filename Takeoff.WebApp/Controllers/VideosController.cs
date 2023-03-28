using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Amazon.S3.Model;
using Mediascend.Web;
using MvcContrib;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Transcoder;
using System.IO;
using Takeoff.ViewModels;
using Amazon.S3;

namespace Takeoff.Controllers
{

    [SubController("/productions/{productionId}/videos", true)]
    public class VideosController : BasicController
    {

        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        [RestActionAttribute]
        public ActionResult Details(int id)
        {
            var video = Things.Get<VideoThing>(id);
            var production = video.FindAncestor<ProjectThing>();//no neeed to verify because AuthorizeProductionAccess handles it.Verify(Permissions.Details);
            var stream = video.GetStream(Request.Browser);
            var user = this.UserThing();
            this.Defer(() => ActivityLogs.LogThingAccess(this.UserId(), this.RequestDate(), video.Id));

            var view = (VideoThingDetailView)video.CreateViewData("Details", this.Identity());
            if (stream != null)
            {
                var location = new S3FileLocation { Location = stream.Location, FileName = stream.FileName };
                view.WatchUrl = location.GetAuthorizedUrl(TimeSpan.FromMinutes(65));
#if DEBUG
                //for local testing...just override the current url
                if (VideoThing.LocalhostVideoUrl.Value.HasChars())
                {
                    view.WatchUrl = VideoThing.LocalhostVideoUrl.Value;
                }
#endif

                //if (logWatch.GetValueOrDefault(true) && stream != null)
                //{
                //    this.Delay<LogController>(c => c.LogWatch(this.RequestDate(), this.UserId(), video.AccountId, production.Id, video.Id, stream.Id, location.Url, !stream.DeletePhysicalFile, stream.Bytes.GetValueOrDefault(), stream.Profile, video.Duration.HasValue ? video.Duration.Value.TotalSeconds : new double?()));
                //}
                var model = new Videos_Details
                {
                    Production = (ProjectThingView)production.CreateViewData(this.Identity()),
                    Video = (VideoThingDetailView)view,
                    IsMember = user != null && user.IsMemberOf(production.Id),
                };
                model.Video.IsSourceDownloadable = true;
                if (model.IsMember)
                {
                    if (model.Production.CanAddComment)
                    {
                        model.CanAddComment = true;
                    }
                    if (model.Production.CanAddCommentReply)
                    {
                        model.CanAddCommentReply = true;
                    }
                    model.CanDownload = video.IsSourceDownloadable;
                }
                this.Identity().IfType<SemiAnonymousUserIdentity>(i =>
                                                                      {
                                                                          model.SemiAnonymousUserName = i.User.UserName;
                                                                          model.CanAddComment = video.GuestsCanComment;
                                                                          model.CanAddCommentReply = model.CanAddComment;
                                                                          model.CanDownload = video.IsSourceDownloadable && model.Video.HasSource;
                                                                      });
                return View(model);
            }
            else
            {
                return null;//todo: return something!
            }
        }

        /// <summary>
        /// Gets the details data (comments, etc) for the video.  Called by production shell when user switches videos.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        public ActionResult Details(int id, bool? logWatch)
        {
            var video = Things.Get<VideoThing>(id);
            var production = video.FindAncestor<ProjectThing>();//no neeed to verify because AuthorizeProductionAccess handles it.Verify(Permissions.Details);
            var stream = video.GetStream(Request.Browser);

            //log that someone accessed this thing
            this.Defer(() => ActivityLogs.LogThingAccess(this.UserId(), this.RequestDate(), video.Id));

            var view = (VideoThingDetailView)video.CreateViewData("Details", this.Identity());
            if (stream != null)
            {
                var location = new S3FileLocation { Location = stream.Location, FileName = stream.FileName };
                //expires in 65 minutes...which I suppose is a good amount of time: todo: lower this if you deploy to amazon ec2  
                view.WatchUrl = location.GetAuthorizedUrl(TimeSpan.FromMinutes(65));
#if DEBUG
                //for local testing...just override the current url
                if (VideoThing.LocalhostVideoUrl.Value.HasChars())
                {
                    view.WatchUrl = VideoThing.LocalhostVideoUrl.Value;
                }
#endif

                if (logWatch.GetValueOrDefault(true) && stream != null)
                {
                    this.Defer(() => ActivityLogs.LogWatch(this.RequestDate(), this.UserId(), video.AccountId, production.Id, video.Id, stream.Id, location.Url, !stream.DeletePhysicalFile, stream.Bytes.GetValueOrDefault(), stream.Profile, video.Duration.HasValue ? video.Duration.Value.TotalSeconds : new double?()));
                }
            }

            return Json(view);
        }


        [HttpPost]
        [RestAction]
        public ActionResult Login(int id, string password)
        {
            Func<ViewResult> viewResult = () =>
                                                {
                                                    return View("Video-Login-SemiAnonymousAccess", new Video_Login
                                                    {
                                                        VideoId = id,
                                                    });
                                                };
            if ( id < 0 )
                ModelState.AddModelError("id", "Invalid ID.");
            if ( !password.HasChars())
                ModelState.AddModelError("password", "Password must have characters.");

            if (!ModelState.IsValid)
                return this.Invalid(viewResult);

            var video = Repos.Videos.Get(id);
            if (video == null)
                return this.DataNotFound();

            //if they are already logged in, it's likely due to a double submit or some kind of weirdness.  The simplest way to handle it is to just log them out
            if (this.Identity() != null)
            {
                this.IdentityService().ClearIdentity(this.HttpContext);
            }

            if (!video.IsGuestAccessEnabled())
            {
                return this.Forbidden(errorCode: ErrorCodes.GuestAccessForbidden, errorDescription: ErrorCodes.GuestAccessForbiddenDescription);
            }

            var decryptedPassword = video.GuestPassword.Decrypt(ApplicationSettings.GuestAccessPasswordIV,
                                                                        ApplicationSettings.GuestAccessPasswordEncryptionKey);
            if (!decryptedPassword.Equals(password))
            {
                this.ModelState.AddModelError("password", "Sorry but that's an invalid password");
                return this.Invalid(viewResult);
            }

            var user = Repos.SemiAnonymousUsers.Instantiate();
            user.Id = Guid.NewGuid();
            user.TargetId = video.Id;
            user.CreatedOn = this.RequestDate();
            Repos.SemiAnonymousUsers.Insert(user);
            this.IdentityService().SetIdentity(new SemiAnonymousUserIdentity(user), IdentityPeristance.TemporaryCookie, this.HttpContext);
            return this.RedirectToAction(c => c.Details(id));
        }


        /// <summary>
        /// Supplies the information needed to post a new file to amazon as a video version.
        /// </summary>
        /// <param name="productionId"></param>
        /// <param name="fileToUpload"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeProductionAccess(ProductionIdParam = "productionId")]
        public ActionResult Allocate(int productionId, FileToUpload fileToUpload)
        {
            Args.ValidKey(productionId, "productionId");
            Args.NotNull(fileToUpload, "fileToUpload");

            var project = Things.Get<ProjectThing>(productionId);
            project.VerifyCreate(typeof(VideoThing), this.UserThing());

            var fileSize = fileToUpload.bytes;
            var user = this.UserThing();
            var account = user.Account;
            var result = CheckLimits(project, user, new FileSize(fileSize));
            if (result != null)
                return result;

            fileToUpload.PrepareForUpload(new S3FileLocation { Location = project.FilesLocation }, FileAccess.Private, true, fileToUpload.bytes);
            return Json(fileToUpload);
        }

        /// <summary>
        /// Called by the client after a successful upload, this adds the necessary database records for the new file(s).
        /// </summary>
        [AuthorizeProductionAccess(ProductionIdParam = "productionId")]
        public ActionResult Create(int productionId, string title, string notes, bool downloadable, string fileKey, string fileName)
        {
            Args.ValidKey(productionId, "productionId");
            Args.HasChars(fileKey, "fileKey");
            Args.HasChars(title, "title");
            
            var production = Things.Get<ProjectThing>(productionId);
            production.VerifyCreate(typeof(VideoThing), this.UserThing());

            //check limits.  this should never be a problem because Allocate will return an error, but it could happen if someone tampers input
            var user = this.UserThing();
            var account = user.Account;

            var location = new S3FileLocation
            {
                Bucket = ConfigUtil.GetRequiredAppSetting("UploadBucket"),
                Key = fileKey
            };

            int fileSize;
            using (var s3Client = new Amazon.S3.AmazonS3Client(Aws.AccessKey, Aws.SecretKey))
            {
                var data = s3Client.GetObjectMetadata(new GetObjectMetadataRequest
                {
                    BucketName = location.Bucket,
                    Key = location.Key
                });
                fileSize = (int)data.ContentLength;
            }

            var result = CheckLimits(production, user, new FileSize(fileSize));
            if (result != null)
                return result;

            var video = new VideoThing
            {
                CreatedByUserId = this.UserThing().Id,
                CreatedOn = this.RequestDate(),
                Title = title,
                IsSourceDownloadable = downloadable
            };
            production.AddChild(video);

            //a single FileThing holds the source video file
            video.AddChild(new FileThing
            {
                CreatedByUserId = this.UserThing().Id,
                OriginalFileName = fileName,
                Location = location.Location,
                FileName = location.FileName,
                Bytes = fileSize,
                DeletePhysicalFile = true,
            });

            object commentActivityContents = null;
            CommentThing comment = null;
            if (notes.HasChars())
            {
                comment = new CommentThing
                {
                    CreatedByUserId = this.UserThing().Id,
                    Body = notes,
                };
                video.AddChild(comment);
            }

            using (var insertBatcher = new CommandBatcher())
            {

                insertBatcher.Executing += (o, e) =>
                    {
                        //we insert the record to log the upload as well as the encoding job
                        insertBatcher.QueueInsertAutoId(new Models.Data.FileUploadLog
                        {
                            AccountId = production.AccountId,
                            Bytes = fileSize,
                            Date = this.RequestDate(),
                            FileThingType = video.Type,
                            Url = location.Url,
                            OriginalFileName = fileName,
                            UserId = this.UserId(),
                            UploadCompleted = this.RequestDate(),
                        }, new SubstituteParameter[]{ new SubstituteParameter
                    {
                        ColumnName = "FileThingId",
                        ValueParameter = video.InsertIdParam
                    }});
                        insertBatcher.QueueInsert(new Models.Data.EncodeLog
                        {
                            AccountId = video.AccountId,
                            InputBytes = fileSize,
                            UploadCompleted = this.RequestDate(),
                            InputOriginalFileName = fileName,
                            UserId = this.UserId(),
                            InputUrl = location.Url,                 
                        }, new SubstituteParameter[]{ new SubstituteParameter
                    {
                        ColumnName = "InputId",
                        ValueParameter = video.InsertIdParam
                    }}, false);
                    };
                video.QueueInsertData(insertBatcher);
                insertBatcher.Execute();
                video.RemoveFromCache();
            }

            if ( comment != null )
                commentActivityContents = comment.GetActivityPanelContents(new Models.Data.ActionSource { Action = "Add" }, true, this.Identity());

            var activityContents = video.GetActivityPanelContents(new Models.Data.ActionSource { Action = "Add" }, true, this.Identity());//that's the only property it needs
            this.DeferRequest(Url.Action<VideosController>(c => c.DispatchTranscodeJob(video.Id, null)));//calls to AWS are offloaded because they can take a while, plus failures can happen.  Mule offloads that work and can retry requests.
            return Json(new
            {
                Data = video.CreateViewData("Details", this.Identity()),
                ActivityPanelItem = activityContents,
                CommentActivityPanelItem = commentActivityContents
            });
        }

        private ActionResult CheckLimits(ProjectThing project, UserThing user, FileSize fileSize)
        {
            AccountThing account = Things.GetOrNull<AccountThing>(project.AccountId);
            if (account == null)
                return null;//something funky
            Func<bool> isUsersAccount = () =>
            {
                var userAccount = user.Account;
                return userAccount != null && account.Id == userAccount.Id;
            };

            var plan = account.Plan;
            if (plan.VideoFileMaxSize.HasValue && plan.VideoFileMaxSize.Value.Bytes > 0 && plan.VideoFileMaxSize <= fileSize)
            {
                return this.RedirectToAction<AccountController>(c => c.LimitReached("videoFileSize", isUsersAccount()));
            }
            if (plan.VideosTotalMaxCount.HasValue && plan.VideosTotalMaxCount.Value <= account.VideoCountBillable)
            {
                return this.RedirectToAction<AccountController>(c => c.LimitReached("videoCount", isUsersAccount()));
            }
            if (plan.VideosPerBillingCycleMax.HasValue && plan.VideosPerBillingCycleMax.Value <= account.VideosAddedInBillingCycle)
            {
                return this.RedirectToAction<AccountController>(c => c.LimitReached("videoUploadCount", isUsersAccount()));
            }
            return null;
        }

        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var entity = Things.Get<VideoThing>(id).Verify(Permissions.Delete, this.UserThing()).Delete();
            return this.Empty();
        }

        /// <summary>
        /// Sends an AWS SQS message to transcode a new version of the video.
        /// </summary>
        /// <param name="videoFile"></param>
        /// <returns></returns>
        [SpecialRestriction(SpecialRestriction.DeferredRequest | SpecialRestriction.Staff | SpecialRestriction.Local)]
        [LogRequest]
        public ActionResult DispatchTranscodeJob(int videoId, string encoder)
        {
            return DispatchTranscodeJobToZencoder(videoId);
        }

        
        private ActionResult DispatchTranscodeJobToSQS(int videoId, string encoder)
        {
            var video = Things.Get<VideoThing>(videoId);
            var production = video.FindAncestor<ProjectThing>();
            var videoFile = video.FindChild<FileThing>();

            var uploaded = new S3FileLocation
            {
                Location = videoFile.Location,
                FileName = videoFile.FileName
            };

            //move the video from the upload bucket to the production bucket.  
            var input = new S3FileLocation
                                  {
                                      Bucket = ConfigUtil.GetRequiredAppSetting("ProductionBucket")
                                  };
            var extension = uploaded.FileName.HasChars() ? System.IO.Path.GetExtension(uploaded.FileName) : string.Empty;
            var key = production.Id.ToInvariant() + "/" + videoId.ToInvariant() + "_" + Guid.NewGuid().StripDashes();
            if (extension.HasChars())
                key = key + extension.StartWith(".");
            input.Key = key;

            using (var s3Client = new Amazon.S3.AmazonS3Client(Aws.AccessKey, Aws.SecretKey))
            {
                s3Client.CopyObject(new CopyObjectRequest
                {
                    CannedACL = S3CannedACL.Private,
                    SourceBucket = uploaded.Bucket,
                    SourceKey = uploaded.Key,
                    DestinationBucket = input.Bucket,
                    DestinationKey = input.Key,
                });
                s3Client.DeleteObject(new DeleteObjectRequest{
                    BucketName = uploaded.Bucket,
                    Key = uploaded.Key,
                });
            }

            videoFile.TrackChanges();
            videoFile.Location = input.Location;
            videoFile.FileName = input.FileName;
            videoFile.Update();

            var outputPrefix = Guid.NewGuid().StripDashes();
            var outputVideo = new S3FileLocation
            {
                Location = videoFile.Location,
                FileName = outputPrefix
            };

            var account = this.Repository<IAccountsRepository>().Get(video.AccountId);
            int durationLimit = account.VideoDurationLimit();

            var encoderParams = new EncodeJobParams
            {
                Encoder = encoder.CharsOr(ConfigUtil.AppSetting("EncodingProvider").CharsOr("Zencoder")),
                InputId = video.Id,
                InputUrl = input.GetAuthorizedUrl(TimeSpan.FromDays(10)),
                InputOriginalFileName = videoFile.OriginalFileName,
                EncodingTriageRequested = this.RequestDate(),
                AccountId = video.AccountId,
                InputFileSize = videoFile.Bytes.GetValueOrDefault(),
                DurationLimit = durationLimit,
                ThumbnailCount = 5,
                ThumbnailMaxHeight = 150,
                ThumbnailMaxWidth = 180,
                ThumbnailAcl = Amazon.S3.S3CannedACL.PublicRead,
                AddNonExpireCacheIfThumbnailAclPublic = true,
                ThumbnailLocation = production.FilesLocation,
                Outputs = new EncodeJobOutputVideo[]{
                    new EncodeJobOutputVideo
                    {
                        Profile = "Web",
                        Url = "s3://" + outputVideo.Bucket + "/" + outputVideo.Key + "-w.mp4",
                        KeyFramesPerSecond = 5,
                        TargetAudioQuality = 3,
                        TargetVideoQuality = 2,
                        MaxFrameRate = 30,
                        Width = 960//the max width of the video.  zencoder charges double for 1280x720, plus we don't go full screen so there's no point really.  
                    },
                    new EncodeJobOutputVideo
                    {
                        Profile = "Mobile",
                        Url = "s3://" + outputVideo.Bucket + "/" + outputVideo.Key + "-m.mp4",
                        KeyFramesPerSecond = 5,
                        TargetAudioBitrate = 96,
                        AudioSampleRate = 44100,                        
                        TargetVideoBitrate = 900,
                        MaxFrameRate = 30,
                        Width = 480,
                        Height = 320
                    },
                },
            };

            Amazon.SQS.Model.SendMessageResult sendResult;
            using (var sqsClient = new Amazon.SQS.AmazonSQSClient(Aws.AccessKey, Aws.SecretKey))
            {
                //hook up to the queue
                var queueRequest = new Amazon.SQS.Model.CreateQueueRequest
                {
                    QueueName = ConfigurationManager.AppSettings["EncodingTriageRequestQueue"]
                };
                var queueResponse = sqsClient.CreateQueue(queueRequest);
                var queueUrl = queueResponse.CreateQueueResult.QueueUrl;
                sendResult = sqsClient.SendMessage(new Amazon.SQS.Model.SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = new JavaScriptSerializer().Serialize(encoderParams)
                }).SendMessageResult;
            }

            Videos.UpdateEncodeLog(encoderParams);
            return this.Empty();
        }

        private ActionResult DispatchTranscodeJobToZencoder(int videoId)
        {
            var video = Things.Get<VideoThing>(videoId);
            var production = video.FindAncestor<ProjectThing>();
            var videoFile = video.FindChild<FileThing>();

            var uploaded = new S3FileLocation
            {
                Location = videoFile.Location,
                FileName = videoFile.FileName
            };

            //move the video from the upload bucket to the production bucket.  
            var input = new S3FileLocation
            {
                Bucket = ConfigUtil.GetRequiredAppSetting("ProductionBucket")
            };
            var extension = uploaded.FileName.HasChars() ? System.IO.Path.GetExtension(uploaded.FileName) : string.Empty;
            var key = production.Id.ToInvariant() + "/" + videoId.ToInvariant() + "_" + Guid.NewGuid().StripDashes();
            if (extension.HasChars())
                key = key + extension.StartWith(".");
            input.Key = key;

            using (var s3Client = new Amazon.S3.AmazonS3Client(Aws.AccessKey, Aws.SecretKey))
            {
                s3Client.CopyObject(new CopyObjectRequest
                {
                    CannedACL = S3CannedACL.Private,
                    SourceBucket = uploaded.Bucket,
                    SourceKey = uploaded.Key,
                    DestinationBucket = input.Bucket,
                    DestinationKey = input.Key,
                });
                s3Client.DeleteObject(new DeleteObjectRequest
                {
                    BucketName = uploaded.Bucket,
                    Key = uploaded.Key,
                });
            }

            videoFile.TrackChanges();
            videoFile.Location = input.Location;
            videoFile.FileName = input.FileName;
            videoFile.Update();

            var outputPrefix = Guid.NewGuid().StripDashes();
            var outputVideo = new S3FileLocation
            {
                Location = videoFile.Location,
                FileName = outputPrefix
            };

            var account = this.Repository<IAccountsRepository>().Get(video.AccountId);
            int durationLimit = account.VideoDurationLimit();

            var encoderParams = new EncodeJobParams
            {
                Encoder = "Zencoder",
                InputId = video.Id,
                InputUrl = input.GetAuthorizedUrl(TimeSpan.FromDays(10)),
                InputOriginalFileName = videoFile.OriginalFileName,
                EncodingTriageRequested = this.RequestDate(),
                EncodingRequested = this.RequestDate(),
                AccountId = video.AccountId,
                InputFileSize = videoFile.Bytes.GetValueOrDefault(),
                DurationLimit = durationLimit,
                ThumbnailCount = 5,
                ThumbnailMaxHeight = 150,
                ThumbnailMaxWidth = 180,
                ThumbnailAcl = Amazon.S3.S3CannedACL.PublicRead,
                AddNonExpireCacheIfThumbnailAclPublic = true,
                ThumbnailLocation = production.FilesLocation,
                Outputs = new EncodeJobOutputVideo[]{
                    new EncodeJobOutputVideo
                    {
                        Profile = "Web",
                        Url = "s3://" + outputVideo.Bucket + "/" + outputVideo.Key + "-w.mp4",
                        KeyFramesPerSecond = 5,
                        TargetAudioQuality = 3,
                        TargetVideoQuality = 2,
                        MaxFrameRate = 30,
                        Width = 960//the max width of the video.  zencoder charges double for 1280x720, plus we don't go full screen so there's no point really.  
                    },
                    new EncodeJobOutputVideo
                    {
                        Profile = "Mobile",
                        Url = "s3://" + outputVideo.Bucket + "/" + outputVideo.Key + "-m.mp4",
                        KeyFramesPerSecond = 5,
                        TargetAudioBitrate = 96,
                        AudioSampleRate = 44100,                        
                        TargetVideoBitrate = 900,
                        MaxFrameRate = 30,
                        Width = 480,
                        Height = 320
                    },
                },
            };



            var thumbLocation = new S3FileLocation
            {
                Location = encoderParams.ThumbnailLocation
            };

            var prefix = Guid.NewGuid().StripDashes();
            var zencoderRequest = new ZencoderCreateJobRequest()
            {
                ApiKey = ConfigurationManager.AppSettings["ZencoderApiKey"],
                Input = encoderParams.InputUrl,
                Test = encoderParams.Test,
                ThumbnailAcl = encoderParams.ThumbnailAcl,
                ThumbnailBaseUrl = "s3://" + thumbLocation.Bucket + "/" + thumbLocation.FolderInBucket,
                ThumbnailCount = encoderParams.ThumbnailCount,
                ThumbnailMaxHeight = encoderParams.ThumbnailMaxHeight,
                ThumbnailMaxWidth = encoderParams.ThumbnailMaxWidth,
                ThumbnailPrefix = prefix,
                Region = "us",
                NotificationUrl = this.Url.UrlPrefixes().FromRelative("/encodes/zencodernotification", UrlType.AbsoluteHttps)
            };

            List<ZencoderOutputParams> zencoderOutputs = new List<ZencoderOutputParams>();
            foreach (var outputRequest in encoderParams.Outputs)
            {
                var zencoderOutput = new ZencoderOutputParams();
                zencoderOutputs.Add(zencoderOutput);
                zencoderOutput.TargetUrl = outputRequest.Url;
                zencoderOutput.Label = outputRequest.Profile;
                zencoderOutput.KeyFramesPerSecond = 5; //todo: get this somewhere else
                outputRequest.AudioSampleRate.IfHasVal(v => zencoderOutput.AudioSampleRate = v);
                outputRequest.Width.IfHasVal(v => zencoderOutput.Width = v);
                outputRequest.Height.IfHasVal(v => zencoderOutput.Height = v);
                outputRequest.TargetAudioBitrate.IfHasVal(v => zencoderOutput.AudioBitrate = v);
                outputRequest.TargetAudioQuality.IfHasVal(v => zencoderOutput.AudioQuality = v);
                outputRequest.AudioSampleRate.IfHasVal(v => zencoderOutput.AudioSampleRate = v);
                outputRequest.TargetVideoBitrate.IfHasVal(v => zencoderOutput.VideoBitrate = v);
                outputRequest.TargetMaxVideoBitrate.IfHasVal(v => zencoderOutput.MaxBitrate = v);
                outputRequest.TargetVideoQuality.IfHasVal(v => zencoderOutput.VideoQuality = v);
                outputRequest.MaxFrameRate.IfHasVal(v => zencoderOutput.MaxFramerate = (int)v);
            }
            zencoderRequest.Outputs = zencoderOutputs.ToArray();

            var client = new ZencoderClient();
            var apiResponse = client.CreateJob(zencoderRequest);
            if (apiResponse == null)
                throw new Exception("Empty api response.");

            encoderParams.EncoderJobId = Convert.ToString(apiResponse["id"]);
            
            Videos.UpdateEncodeLog(encoderParams);


            return this.Empty();
        }


        [DeferredRequestOnly]
        public ActionResult ProcessEncodeResult(EncodeJobParams result)
        {
            int videoId = result.InputId;
            //first thing we do is check to make sure the video hasn't already been marked as completed or screwed up somehow.  this happened a bit in the past, and ideally wouldn't happen in a perfect world
            using (var db = DataModel.ReadOnly)
            {
                var upload = (from tj in db.EncodeLogs where tj.InputId == result.InputId select tj).SingleOrDefault();
                if (upload == null || upload.JobCompleted.HasValue)
                {
                    return new EmptyResult();
                }
            }

            var error = result.ErrorCode != TranscodeJobErrorCodes.NotSet;

            var video = Things.GetOrNull<VideoThing>(videoId);
            ProjectThing production = video == null ? null : video.FindAncestor<ProjectThing>();
            FileThing videoFile = video == null ? null : video.FindChild<FileThing>();

            //if production or video is null, they deleted it before the transcoding completed.  in this case we just kill the file and go on
            if (video == null || error)
            {
                if (result.Outputs != null && result.Outputs.Length > 0)
                {
                    result.Outputs.Each(target =>
                    {
                        if (target.Url.HasChars())
                        {
                            S3.TryDeleteFile(target.Url);
                        }
                    });
                }
                if (result.Thumbnails != null && result.Thumbnails.Length > 0)
                {
                    result.Thumbnails.Each(target =>
                    {
                        if (target.Url.HasChars())
                        {
                            S3.TryDeleteFile(target.Url);
                        }
                    });
                }

                if (video != null)
                {
                    this.DeferRequest<EmailController>(c => c.SendProductionVideoTranscodeError(video.CreatedByUserId, production.Id, videoFile.OriginalFileName, result.ErrorCode, video.Title));
                    video.TrackChanges();
                    video.IsComplimentary = true;//this is necessary to ensure Account.VideosAddedInBillingCycle is right.  
                    video.Update();
                    video.Delete();
                }
            }
            else
            {
                Debug.Assert(result.EncodingCompleted.HasValue);

                using (var insertBatcher = new CommandBatcher())
                {
                    var now = this.RequestDate();
                    foreach (var outputVideo in result.Outputs)
                    {
                        var videoStream = video.AddChild(new VideoStreamThing
                        {
                            CreatedByUserId = video.CreatedByUserId,
                            CreatedOn = now,
                            VideoBitRate = outputVideo.ActualVideoBitRate,
                            AudioBitRate = outputVideo.ActualAudioBitRate,
                            DeletePhysicalFile = true,
                            Profile = outputVideo.Profile,
                        });

                        videoStream.Url = outputVideo.Url;
                        videoStream.Bytes = outputVideo.FileSize;
                        videoStream.QueueInsertData(insertBatcher);
                    }

                    if (result.Thumbnails != null)
                    {
                        foreach (var thumbnail in result.Thumbnails)
                        {
                            video.AddChild(new VideoThumbnailThing
                                {
                                    CreatedByUserId = video.CreatedByUserId,
                                    CreatedOn = now,
                                    Url = thumbnail.Url,
                                    Time = thumbnail.Time,
                                    Width = thumbnail.Width,
                                    Height = thumbnail.Height,
                                    DeletePhysicalFile = true,
                                    LogInsertActivity = false,
                                }).QueueInsertData(insertBatcher);
                        }
                    }
                    insertBatcher.Execute();

                    //could maybe have saved a db command by putting UpdateEncodeLog before the update, but it's only one small db hit and the job isn't done quite yet
                    using (var db = DataModel.ReadOnly)
                    {
                        (from t in db.EncodeLogs where t.InputId == result.InputId select t.InputDuration).Single().IfHasVal(v => video.Duration = TimeSpan.FromSeconds(v));
                    }
                    var changes = new Dictionary<string,object>();
                    changes["HasVideo"] = video.HasVideo;
                    changes["Thumbnails"] = video.Children.OfType<VideoThumbnailThing>().OrderBy(t => t.Time).Select(c => (VideoThumbnailThingView)c.CreateViewData(this.Identity())).ToArray();
                    video.Update(true, null, changes);
                }

                this.DeferRequest<EmailController>(c => c.NotifyForNewVideo(video.Id));
            }

            result.JobCompleted = DateTime.UtcNow;
            Videos.UpdateEncodeLog(result);

            return new EmptyResult();
        }

        /// <summary>
        /// Called after the video has been triaged and a possible request to zencoder has been made.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="errorCode"></param>
        /// <param name="externalProcessorId"></param>
        /// <returns></returns>
        [DeferredRequestOnly]
        public ActionResult UpdateEncodedJob(EncodeJobParams jobParams)
        {
            //this will update all the metadata and other that was updated since it was dispatched from here
            Videos.UpdateEncodeLog(jobParams);

            var video = Things.GetOrNull<VideoThing>(jobParams.InputId);
            ProjectThing production = video == null ? null : video.FindAncestor<ProjectThing>();
            FileThing videoFile = video == null ? null : video.FindChild<FileThing>();

            //if production or video is null, they deleted it before the transcoding completed.  in this case we just kill the file and go on
            if (video == null || jobParams.ErrorCode != TranscodeJobErrorCodes.NotSet)
            {
                if (video != null)
                {
                    this.DeferRequest<EmailController>(c => c.SendProductionVideoTranscodeError(video.CreatedByUserId, production.Id, videoFile.OriginalFileName, jobParams.ErrorCode, video.Title));
                    video.Delete();
                }
            }
            return new EmptyResult();
        }

        /// <summary>
        /// Returns transcode jobs that are currently being encoded by Zencoder.
        /// </summary>
        /// <returns></returns>
        [DeferredRequestOnly]
        [Obsolete]
        public ActionResult VideosBeingEncoded()
        {
            var videos = Videos.VideosBeingEncoded();
            if (videos.HasItems())
                return Json(videos);
            return this.Empty();
        }

        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        [RestActionAttribute]
        public ActionResult Download(int id)
        {
            var video = Things.GetOrNull<VideoThing>(id);
            if (video == null)
                return this.DataNotFound();

            var production = video.FindAncestor<ProjectThing>();//no need to verify because of AuthorizeProductionAccess

            if (!video.IsSourceDownloadable)
            {
                return this.Forbidden(errorCode: ErrorCodes.VideoNotDownloadable, errorDescription: ErrorCodes.VideoNotDownloadableDescription);
            };

            if (!video.IsSourceDownloadable)
                throw new InvalidOperationException("Video source was not downloadable.");

            var file = video.ChildrenOfType<FileThing>().FirstOrDefault();
            if (file == null)
                throw new Exception("No video file found.");

            var location = new S3FileLocation { Location = file.Location, FileName = file.FileName };
            this.Defer(() => ActivityLogs.LogDownload(this.UserId(), this.RequestDate(), file.AccountId, file.Id, file.Type, location.Url, file.Bytes.GetValueOrDefault(), file.OriginalFileName, !file.DeletePhysicalFile));
            using(var s3 = new Amazon.S3.AmazonS3Client(Aws.AccessKey,Aws.SecretKey))
            {
                var filename = video.Title;
                if ( !filename.HasChars())
                    filename = "video";
                //add the extension if the original file had one and it's not already in the video title
                if (file.OriginalFileName.HasChars() && Path.GetExtension(file.OriginalFileName).HasChars() && !(Path.GetExtension(filename).HasChars() && Path.GetExtension(filename) == Path.GetExtension(file.OriginalFileName)))
                    filename = filename + Path.GetExtension(file.OriginalFileName).StartWith(".");
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


        [AuthorizeProductionAccess(DescendantIdParam = "id")]
        [HttpPost]
        public ActionResult Edit(int id, Videos_Edit model)
        {
            if ( !ModelState.IsValid)
                return this.Invalid();

            var video = (VideoThing)Things.GetOrNull<VideoThing>(id);
            if ( video == null)
                return this.DataNotFound();            
            if (!video.HasPermission(Permissions.Edit, this.UserThing()))
                return this.Forbidden();

            video.TrackChanges();

            if (!model.CustomUrl.EqualsCaseSensitive(video.CustomUrl))
            {
                if (model.CustomUrl.HasChars())
                {
                    if (model.CustomUrl.HasChars() && !VantiyUrlHelper.IsValid(model.CustomUrl))
                    {
                        this.ModelState.AddModelError("CustomUrl", "Only letters, numbers, underscores, and dashes are allowed.");
                    }
                    if (ApplicationSettings.ForbiddenVanityUrls.Contains(model.CustomUrl))
                    {
                        this.ModelState.AddModelError(model.PropertyName(m => m.CustomUrl), "That url is not available.");
                    }
                    else if (VantiyUrlHelper.IsUrlTaken(model.CustomUrl))
                    {
                        this.ModelState.AddModelError("CustomUrl", "That url is not available.");
                    }
                    else
                    {
                        video.CustomUrl = model.CustomUrl;
                    }
                }
                else
                {
                    video.CustomUrl = null;
                }
            }

            if (!ModelState.IsValid)
                return this.Invalid();


            video.Title = model.Title;
            if (model.IsDownloadable.HasValue)
            {
                video.IsSourceDownloadable = model.IsDownloadable.Value;
            }
            video.GuestPassword = model.GuestPassword.HasChars()
                                              ? model.GuestPassword.EncryptTwoWay(
                                                  ApplicationSettings.GuestAccessPasswordIV,
                                                  ApplicationSettings.GuestAccessPasswordEncryptionKey)
                                              : null;
            model.GuestsCanComment.IfHasVal(v => video.GuestsCanComment = v);
            
            video.Update();

            return this.StatusCode(HttpStatusCode.OK);
        }



    }
}
