using System.Data;
using System.Linq;
using System.Security;
using System.Web.Mvc;

using Mediascend.Web;
using MvcContrib;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Controllers;
using System;
using System.Web.Script.Serialization;
using System.Web;
using Takeoff.Platform;
using System.Drawing;
using Takeoff.ViewModels;
using Takeoff.Resources;
using System.Configuration;

namespace Takeoff.Controllers
{

    public class ProductionsController : BasicController
    {
        static Size MaxLogoSize = new Size(300, 70);
        static long LogoQuality = 70;


        /// <summary>
        /// Gets the details of a production.  
        /// </summary>
        /// <param name="id"></param>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [AuthorizeProductionAccess]
        public ActionResult Details(int id, int? videoId = null, string focus = null, string message = null, string messageType = null)
        {
            var production = Things.GetOrNull<ProjectThing>(id);
            
            var currUser = this.UserThing();
            var identity = this.Identity();

            ViewData["message"] = message;
            ViewData["messageType"] = messageType;
            var viewData = (ProjectThingView)production.CreateViewData(identity);

            //try and get the videoId from the focus string
            if (!videoId.HasValue && focus.HasChars())
            {
                var focusParams = focus.Split('_');
                for (var i = 0; i < focusParams.Length; i += 2)
                {
                    if ("video".Equals(focusParams[i], StringComparison.OrdinalIgnoreCase))
                    {
                        videoId = focusParams[i + 1].ToIntTry();
                        break;
                    }
                }
            }

            this.Defer(() => ActivityLogs.LogThingAccess(this.UserId(), this.RequestDate(), id));

            if (videoId.HasValue)
            {
                var video = (VideoThing)production.FindDescendantById(videoId.Value);
                if (video != null)
                {
                    this.Defer(() => ActivityLogs.LogThingAccess(this.UserId(), this.RequestDate(), video.Id));
                    var stream = video.GetStream(Request.Browser);
                    //have to fill CurrentVideo because right now there isn't a way to pass parameters like that into Thing CreateViewData
                    viewData.CurrentVideo = (VideoThingDetailView)video.CreateViewData("Details", this.Identity());
                    if (stream != null)
                    {
                        var location = new S3FileLocation { Location = stream.Location, FileName = stream.FileName };
                        //expires in 65 minutes...which I suppose is a good amount of time: todo: lower this if you deploy to amazon ec2  
                        viewData.CurrentVideo.WatchUrl = location.GetAuthorizedUrl(TimeSpan.FromMinutes(65));
#if DEBUG
                        //for local testing
                        if (VideoThing.LocalhostVideoUrl.Value.HasChars())
                        {
                            viewData.CurrentVideo.WatchUrl = VideoThing.LocalhostVideoUrl.Value;
                        }
#endif
                        this.Defer(() => ActivityLogs.LogWatch(this.RequestDate(), this.UserId(), video.AccountId, production.Id, video.Id, stream.Id, location.Url, !stream.DeletePhysicalFile, stream.Bytes.GetValueOrDefault(), stream.Profile, video.Duration.HasValue ? video.Duration.Value.TotalSeconds : new double?()));
                    }
                }
            }

            return CreateDetailsResult(id, focus, message, messageType, currUser, production, viewData);
        }

        private ActionResult CreateDetailsResult(int id, string focus, string message, string messageType, UserThing currUser,
                                                 ProjectThing production, ProjectThingView viewData)
        {
            var model = new Production_Details
                            {
                                Data = viewData,
                                EnableChangeSyncing = true,
                                ActivityInterval = ApplicationSettings.EnableAjaxPolling ? (Request.Browser.IsMobileDevice ? TimeSpan.FromSeconds(60): TimeSpan.FromSeconds(3)) : TimeSpan.FromDays(2),
                                InitialFocus = focus,
                                IsMember = currUser != null && currUser.IsMemberOf(id),
                                StartupMessageBody = message,
                                PreferHtml5Video = false,
                                QuickAccessDecryptedPassword =
                                    production.GuestPassword.HasChars()
                                        ? production.GuestPassword.Decrypt(ApplicationSettings.GuestAccessPasswordIV,
                                                                                 ApplicationSettings.GuestAccessPasswordEncryptionKey)
                                        : string.Empty,
                                QuickAccessAllowCommenting = production.GuestsCanComment,
                                CustomUrl = production.VanityUrl,
                                FilePickerApiKey = ApplicationSettings.FilePickerApiKey,
                                StartupMessageType =
                                    messageType.HasChars()
                                        ? Enum<StartupMessageType>.Parse(messageType, true)
                                        : StartupMessageType.Success,
                            };
            this.Identity().IfType<SemiAnonymousUserIdentity>(i => model.SemiAnonymousUserName = i.User.UserName);

            return View("Details", model);
        }



        [HttpPost]
        [RestAction]
        public ActionResult Login(int id, string password)
        {
            var production = Repos.Productions.Get(id);
            if (production == null)
                return this.DataNotFound();

            Func<ViewResult> viewResult = () =>
            {
                return View(production.IsGuestAccessEnabled() ? "Production-Login-SemiAnonymousAccess" : "Production-Login", new Productions_Login
                                                {
                                                    ProductionId = id,
                                                });
            };
            if (id < 0)
                ModelState.AddModelError("id", "Invalid ID.");
            if (!password.HasChars())
                ModelState.AddModelError("password", "Password must have characters.");

            if (!ModelState.IsValid)
                return this.Invalid(viewResult);


            //if they are already logged in, it's likely due to a double submit or some kind of weirdness.  The simplest way to handle it is to just log them out
            if (this.Identity() != null)
            {
                this.IdentityService().ClearIdentity(this.HttpContext);
            }

            if (!production.IsGuestAccessEnabled())
            {
                return this.Forbidden(errorCode: ErrorCodes.GuestAccessForbidden, errorDescription: ErrorCodes.GuestAccessForbiddenDescription);
            }

            var decryptedPassword = production.GuestPassword.Decrypt(ApplicationSettings.GuestAccessPasswordIV,
                                                                        ApplicationSettings.GuestAccessPasswordEncryptionKey);
            if (!decryptedPassword.EqualsCaseSensitive(password))
            {
                this.ModelState.AddModelError("password", "Sorry but that's an invalid password");
                return this.Invalid(viewResult);
            }

            var user = Repos.SemiAnonymousUsers.Instantiate();
            user.Id = Guid.NewGuid();
            user.TargetId = production.Id;
            user.CreatedOn = this.RequestDate();
            Repos.SemiAnonymousUsers.Insert(user);
            this.IdentityService().SetIdentity(new SemiAnonymousUserIdentity(user), IdentityPeristance.TemporaryCookie, this.HttpContext);
            return this.RedirectToAction(c => c.Details(id, null, null, null, null));
        }


        [HttpGet]
        [RestrictIdentity]
        public ActionResult Create()
        {
            var user = this.UserThing();

            var account = user.Account;
            if (account == null)
                return View("Create.NoAccount");

            var result = CheckLimits(account, user);
            if (result != null)
                return result;

            return View(new Productions_Create
                            {
                            });
        }

        [RestrictIdentity]
        [HttpPost]
        [ValidateInput(false)]//todo: only mark it on the title attribute
        [ValidateAntiForgeryToken]
        public ActionResult Create(Productions_Create model)
        {
            if (!ModelState.IsValid)
            {
                return this.Invalid(() => View(model));
            }
            var user = this.UserThing();
            var account = user.Account;//in the future if you do multiple accounts, make sure you check permission:  account.VerifyCreate(typeof(ProjectThing));

            var result = CheckLimits(account, user);
            if (result != null)
                return result;

            var production = new ProjectThing
            {
                CreatedByUserId = user.Id,
                IsContainer = true,
                AccountId = account.Id,
                ContainerId = account.Id,
                Title = model.Title,
            };


            if (model.CustomUrl.HasChars())
            {
                if (model.CustomUrl.HasChars() && !VantiyUrlHelper.IsValid(model.CustomUrl))
                {
                    this.ModelState.AddModelError("CustomUrl", "Only letters, numbers, underscores, and dashes are allowed.");
                }
                if (ApplicationSettings.ForbiddenVanityUrls.Contains(model.CustomUrl))
                {
                    this.ModelState.AddModelError("CustomUrl", "That url is not available.");
                }
                else if (VantiyUrlHelper.IsUrlTaken(model.CustomUrl))
                {
                    this.ModelState.AddModelError("CustomUrl", "That url is not available.");
                }
                if ( !this.ModelState.IsValid)
                {
                    return View(model);
                }
                production.VanityUrl = model.CustomUrl;
            }

            if (model.SemiAnonymousDecryptedPassword.HasChars(CharsThatMatter.NonWhitespace))
            {
                production.GuestPassword = model.SemiAnonymousDecryptedPassword.Trim().EncryptTwoWay(ApplicationSettings.GuestAccessPasswordIV, ApplicationSettings.GuestAccessPasswordEncryptionKey);
                production.GuestsCanComment = model.SemiAnonymousUsersCanComment;
            }
            
            var membership = new MembershipThing
            {
                CreatedByUserId = user.Id,
                UserId = user.Id,
            };
            production.AddChild(membership);

            production.Insert();

            //create a copy of the membership underneath the user
            //user.AddChild(membership.CreateLinkedThing<MembershipThing>()).Insert();

            Productions.SetFilesLocationForAfterInsert(production);

            if (model.Logo != null && model.Logo.ContentLength.IsPositive())
            {
                var fileStream = model.Logo.InputStream;
                string logoFileLocation = production.FilesLocation;

                var nameAndSize = WebImageUtil.ResizeAndUpload(fileStream, production.FilesLocation, MaxLogoSize, LogoQuality);
                if (nameAndSize == null)
                {
                    ModelState.AddModelError("Logo", Strings.Productions_Create_InvalidImageType_Msg);
                    production.Delete(true);
                    return View(model);
                }
                var fileName = nameAndSize.Item1;
                var size = nameAndSize.Item2;
                //delete any current specific logo
                if (production.LogoImageId.HasValue)
                {
                    var logo = production.FindDescendantById(production.LogoImageId.Value);
                    if (logo != null)
                        logo.Delete();
                    production.LogoImageId = null;
                }

                //contains the basic properties for the logo.  will not have an ID set yet
                var logoThing = new ImageThing
                {
                    CreatedByUserId = this.UserId(),
                    CreatedOn = this.RequestDate(),
                    Location = production.FilesLocation,
                    FileName = fileName,
                    OriginalFileName = logoFileLocation,
                    DeletePhysicalFile = true,
                    Height = size.Height,
                    Width = size.Width,
                };

                production.LogoImageId = production.AddChild<ImageThing>(logoThing).Insert().Id;
                production.Update();
            }

            return this.RedirectToAction<ProductionsController>(o => o.Details(production.Id, null, null, null, null));
        }

        /// <summary>
        /// Checks limits for the current plan, a demo plan, or if they aren't verified.  If the limit has been reached, 
        /// If the user isn't verified, this checks whether they can upload the given video.  If not, it returls an error result.
        /// </summary>
        /// <returns></returns>
        private ActionResult CheckLimits(IAccount account, IUser user)
        {
            if (account == null)
                return null;//something funky
            Func<bool> isUsersAccount = () =>
            {
                var userAccount = user.Account;
                return userAccount != null && account.Id == userAccount.Id;
            };

            var plan = account.Plan;
            if (plan.ProductionLimit.HasValue && account.ProductionCountBillable >= plan.ProductionLimit.Value)
            {
                return this.RedirectToAction<AccountController>(c => c.LimitReached("productions", isUsersAccount()));
            }

            return null;
        }


        [AuthorizeProductionAccess]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Productions_Edit model)
        {
            Args.HasChars(model.Title, "title");
            Args.GreaterThanZero(id, "id");
            var production = (ProjectThing)Things.Get<ProjectThing>(id).Verify(Permissions.Edit, this.UserThing());
            if (!ModelState.IsValid)
            {
                return CreateDetailsResult(id, "settings", "Uh oh, there were some problems.", "error", this.UserThing(), production, (ProjectThingView)production.CreateViewData(this.Identity()));
            }
            production.TrackChanges();

            if (!model.CustomUrl.EqualsCaseSensitive(production.VanityUrl))
            {
                if (model.CustomUrl.HasChars())
                {
                    if (model.CustomUrl.HasChars() && !VantiyUrlHelper.IsValid(model.CustomUrl))
                    {
                        this.ModelState.AddModelError("CustomUrl", "Only letters, numbers, underscores, and dashes are allowed.");
                    }
                    if (ApplicationSettings.ForbiddenVanityUrls.Contains(model.CustomUrl))
                    {
                        this.ModelState.AddModelError("CustomUrl", "That url is not available.");
                    }
                    else if (VantiyUrlHelper.IsUrlTaken(model.CustomUrl))
                    {
                        this.ModelState.AddModelError("CustomUrl", "That url is not available.");
                    }
                    else
                    {
                        production.VanityUrl = model.CustomUrl;
                    }
                }
                else
                {
                    production.VanityUrl = null;
                }
                if (!ModelState.IsValid)
                {
                    return CreateDetailsResult(id, "settings", "Uh oh, there were some problems.", "error", this.UserThing(), production, (ProjectThingView)production.CreateViewData(this.Identity()));
                }
            }
            
            
            if (model.Logo != null)
            {
                var fileStream = model.Logo.InputStream;
                AccountThing account = null;
                string logoFileLocation = production.FilesLocation;

                var nameAndSize = WebImageUtil.ResizeAndUpload(fileStream, production.FilesLocation, MaxLogoSize, LogoQuality);
                if (nameAndSize == null)
                {
                    return this.RedirectToAction<ProductionsController>(c => c.Details(id, null, "settings", "The file was not an image we could detect.  Try uploading in jpg, png, gif, or tiff format.", "error"));
                }
                var fileName = nameAndSize.Item1;
                var size = nameAndSize.Item2;
                //delete any current specific logo
                if (production.LogoImageId.HasValue)
                {
                    var logo = production.FindDescendantById(production.LogoImageId.Value);
                    if (logo != null)
                        logo.Delete();
                    production.LogoImageId = null;
                }

                //contains the basic properties for the logo.  will not have an ID set yet
                var logoThing = new ImageThing
                    {
                        CreatedByUserId = this.UserId(),
                        CreatedOn = DateTime.UtcNow,
                        Location = production.FilesLocation,
                        FileName = fileName,
                        OriginalFileName = logoFileLocation,
                        DeletePhysicalFile = true,
                        Height = size.Height,
                        Width = size.Width,
                    };

                production.LogoImageId = production.AddChild<ImageThing>(logoThing).Insert().Id;
            }

            if (!model.Title.EqualsCaseSensitive(production.Title))
                production.Title = model.Title;

            var productionAccount = Things.GetOrNull<AccountThing>(production.AccountId);

                //massage quick access pw a bit.  it will never be an empty string
                if (model.QuickAccessDecryptedPassword != null)
                    model.QuickAccessDecryptedPassword = model.QuickAccessDecryptedPassword.Trim();
                if (!model.QuickAccessDecryptedPassword.HasChars())
                    model.QuickAccessDecryptedPassword = null;

                string currentPassword = production.GuestPassword.HasChars()
                                          ? production.GuestPassword.Decrypt(ApplicationSettings.GuestAccessPasswordIV,
                                                                                   ApplicationSettings.GuestAccessPasswordEncryptionKey)
                                          : null;

                if (!currentPassword.EqualsCaseSensitive(model.QuickAccessDecryptedPassword))
                {
                    production.GuestPassword = model.QuickAccessDecryptedPassword == null
                                                         ? null
                                                         : model.QuickAccessDecryptedPassword.EncryptTwoWay(
                                                             ApplicationSettings.GuestAccessPasswordIV,
                                                             ApplicationSettings.GuestAccessPasswordEncryptionKey);
                }

                if ( model.QuickAccessAllowCommenting.GetValueOrDefault() != production.GuestsCanComment)
                {
                    production.GuestsCanComment = model.QuickAccessAllowCommenting.GetValueOrDefault();
                }
        
            if (this.ModelState.IsValid)
            {
                production.Update();
                return
                    this.RedirectToAction<ProductionsController>(
                        c => c.Details(id, null, "settings", "Your changes have been made.", "success"));
            }
            else
            {

                return CreateDetailsResult(id, "settings", "Uh oh, there were some problems.", "error", this.UserThing(), production, (ProjectThingView)production.CreateViewData(this.Identity()));                
            }
        }


        [AuthorizeProductionAccess]
        [HttpPost]
        public ActionResult Delete(int id, string what)
        {
            var entity = Things.Get<ProjectThing>(id);
            if (string.IsNullOrEmpty(what))
            {
                entity.Verify(Permissions.Delete, this.UserThing()).Delete();
                return this.RedirectToAction<DashboardController>(c => c.Index("The production has been deleted.", null));
            }
            else if (what.Trim().Equals("logo", StringComparison.OrdinalIgnoreCase))
            {
                entity.Verify(Permissions.Edit, this.UserThing());
                if (entity.LogoImageId.HasValue)
                {
                    entity.TrackChanges();
                    entity.FindDescendantById(entity.LogoImageId.Value).Delete();
                    entity.LogoImageId = null;
                    entity.Update();
                }
                else
                {
                    var account = Things.Get<AccountThing>(entity.AccountId);
                    account.TrackChanges();
                    if (account != null && account.LogoImageId.HasValue)
                    {
                        account.FindDescendantById(account.LogoImageId.Value).Delete();
                        account.LogoImageId = null;
                        account.Update();
                    }
                }
                return this.Empty();
            }
            else
            {
                throw new InvalidOperationException("what parameter was wrong");
            }
        }


        [RestActionAttribute]
        [AuthorizeProductionAccess]
        public ActionResult Changes(int id, int? latestChangeId)
        {
            var production = Things.GetOrNull<ProjectThing>(id);

            return Json(ThingChanges.GetChanges(production, latestChangeId.GetValueOrDefault(), this.Identity()));
        }


        //public ActionResult TestDb(int id)
        //{
        //    using (var db = DataModel.ReadOnly)
        //    {
        //        var production = (from p in db.Projects
        //                          join pt in db.Things on p.ThingId equals pt.Id
        //                          where p.ThingId == id && pt.DeletedOn == null
        //                          select new
        //                                     {
        //                                         pt.AccountId,
        //                                         pt.ContainerId,
        //                                         pt.CreatedByUserId,
        //                                         pt.CreatedOn,
        //                                         pt.OwnerUserId,
        //                                         p.Title,
        //                                         Videos = (
        //                                            from v in db.Videos
        //                                            join vt in db.Things on v.ThingId equals vt.Id
        //                                            where vt.ParentId == pt.Id && vt.DeletedOn == null
        //                                            select new
        //                                                       {
        //                                                           vt.CreatedByUserId,
        //                                                           vt.CreatedOn,
        //                                                           vt.DeletedOn,
        //                                                           vt.OwnerUserId,
        //                                                           v.Title,
        //                                                           Thumbnails = (
        //                                                               from videoThumbnail in db.VideoThumbnails
        //                                                               join image in db.Images on videoThumbnail.ThingId equals image.ThingId
        //                                                               join file in db.Files on videoThumbnail.ThingId equals file.ThingId
        //                                                               join videoThumbnailThing in db.Things on videoThumbnail.ThingId equals videoThumbnailThing.Id
        //                                                               where videoThumbnailThing.ParentId == vt.Id && videoThumbnailThing.DeletedOn == null
        //                                                               select new
        //                                                               {
        //                                                                   image.Height,
        //                                                                   image.Width,
        //                                                                   videoThumbnail.Time,
        //                                                                   file.FileName,
        //                                                                   file.Location,
        //                                                               }
        //                                                           ).ToArray(),
        //                                                           Comments = (
        //                                                               from comment in db.Comments
        //                                                               join commentThing in db.Things on comment.ThingId equals commentThing.Id
        //                                                               where commentThing.ParentId == vt.Id && commentThing.DeletedOn == null
        //                                                               select new
        //                                                               {
        //                                                                   commentThing.CreatedByUserId,
        //                                                                   commentThing.CreatedOn,
        //                                                                   comment.Body,
        //                                                               }
        //                                                           ).ToArray(),
        //                                                       }).ToArray(),
        //                                     }
        //                                     ).SingleOrDefault();
        //        return Json(production, JsonRequestBehavior.AllowGet);
        //    }
        //    return null;
        //}


    }




}
