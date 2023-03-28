using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Amazon.S3.Model;
using Amazon.SimpleDB.Model;
using Amazon.SimpleEmail.Model;
using Mediascend.Web;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using Takeoff.Data;
using Takeoff.Models;
using System.Diagnostics;
using System.Web.Script.Serialization;
using Takeoff.Models.Data;
using Takeoff.Transcoder;
using Takeoff.ViewModels;
using TweetSharp;
using PostmarkDotNet;

namespace Takeoff.Controllers
{
    /// <summary>
    /// Responsible for sending out emails to users.
    /// </summary>
    [ValidateInput(false)]//so some chars like brackets can be included in parameters like an invitation note.  they are html escaped...or at least they better be!
    public class EmailController : BasicController
    {
        public ActionResult Track(Guid id)
        {
            this.Defer(() =>OutgoingMail.LogOpen(id, this.RequestDate()));

            return new FileContentResult(TrackOpenPngBytes, "image/png");
        }

        private static readonly byte[] TrackOpenPngBytes =
            typeof(EmailController).GetEmbeddedResourceBytes("Takeoff.Resources.1x1-transparent.png");

        /// <summary>
        /// Sends the email to the user after they requested a new password.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult PasswordReset(string email)
        {
            var user = Repos.Users.GetByEmail(email);
            var model = new Email_User_PasswordReset
                            {
                                ResetUrl = Url.Action2("PasswordReset", "Account", new { email = user.Email, resetKey = user.PasswordResetKey }, UrlType.AbsoluteHttps),
                            };
            RenderAndSendEmail(user, "User-PasswordReset", model);
            return new EmptyResult();
        }

        /// <summary> 
        /// Sends the email that provides a link to verify their account.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult SendUserVerify(int userId)
        {
            var user = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
            var url = Url.Action2("Verify", "Account", new { verificationKey = user.VerificationKey, email = user.Email }, UrlType.AbsoluteHttps);

            RenderAndSendEmail(user, "User-Verify", new Email_User_Verify{ VerifyUrl = url, });
            return new EmptyResult();
        }


        public ActionResult PreviewHtml(string view, bool inlinecss, bool send)
        {
            EmailResult emailResult = new EmailResult();
            emailResult.ViewName = view;
            emailResult.ViewData = base.ViewData;
            emailResult.ViewData["html"] = true;
            emailResult.TempData = base.TempData;
            emailResult.ExecuteResult(this.ControllerContext);
            var html = emailResult.Source.Before("</html>").EndWith("</html>");
            if (inlinecss)
            {
                html = CssInliner.CssInliner.GetHtml(CssInliner.CssInliner.InlineHtml3(html, null));
            }
            return new ContentResult
                       {
                           Content = html,
                           ContentType = "text/html"
                       };
        }

        /// <summary>
        /// Sends the email that provides a link to verify their account.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult SendUserSignup(int userId)
        {
            var user = Things.GetOrNull<UserThing>(userId);
            if (user == null)
                return new EmptyResult();

            var model = new Email_User_Signup_MustVerify
                            {
                                VerifyUrl = Url.Action2("Verify", "Account", new { verificationKey = user.VerificationKey, email = user.Email }, UrlType.AbsoluteHttps),
                            };
            RenderAndSendEmail(user, "User-Signup-MustVerify", model);

            return new EmptyResult();
        }


        [DeferredRequestOrLocalOnly]
        public ActionResult SendMembershipRequest(int requestId)
        {
            var request = Things.GetOrNull<MembershipRequestThing>(requestId);
            if (request == null)
                return new EmptyResult();
            var production = request.FindAncestor<ProjectThing>();
            var invitedBy = Things.GetOrNull<UserThing>(request.CreatedByUserId);
            var invitee = Things.GetOrNull<UserThing>(request.InviteeId);
            var productionOwner = production.Owner;

            MailAddress replyTo = null;
            string sendTo;
            if (request.IsInvitation)
            {
                if (invitee.CreatedByUserId == invitedBy.Id && !invitee.IsVerified)
                {
                    var model = new Email_AddUserToProduction
                                    {
                                        AddedByName = invitedBy.DisplayName,
                                        AddedByEmail = invitedBy.Email,
                                        Note = request.Note,
                                        ProductionUrl = Url.Action2("Verify", "Account", new { verificationKey = invitee.VerificationKey, email = invitee.Email }, UrlType.AbsoluteHttps),
                                    }.Once(m => FillProductionModel(m, production));
                    RenderAndSendEmail(invitee, "ProductionMember-Invited-NewUser", model, invitedBy.Email, invitedBy.DisplayName);
                }
                else
                {
                    var model = new Email_AddUserToProduction
                    {
                        AddedByName = invitedBy.DisplayName,
                        AddedByEmail = invitedBy.Email,
                        Note = request.Note,
                    }.Once(m => FillProductionModel(m, production));
                    RenderAndSendEmail(invitee, "ProductionMember-Invited-ExistingUser", model, invitedBy.Email, invitedBy.DisplayName);
                }
            }
            else
            {
                var model = new Email_MembershipRequestToProduction
                                {
                                    RequestUrl = Url.Action<MembershipRequestsController>(r => r.Details(requestId), UrlType.AbsoluteHttps),
                                    RequestToEmail = productionOwner.Email,
                                    RequestToName = productionOwner.DisplayName,
                                    RequestFromEmail = invitee.Email,
                                    RequestFromName = invitee.DisplayName,
                                    Note = request.Note,
                                }.Once(m => FillProductionModel(m, production));
                RenderAndSendEmail(productionOwner, "ProductionMember-RequestByNonMember", model, invitee.Email, invitee.DisplayName);
            }
            return new EmptyResult();
        }


        /// <summary>
        /// Sends the email to a person indicating a new member to the production.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult SendNewProductionMember(int userId, int productionId, int addedBy, bool emailNewMember)
        {
            var user = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
            var production = Things.Get<ProjectThing>(productionId);
            var addedByUser = Things.Get<UserThing>(addedBy);

            if (emailNewMember)
            {
                var model = new Email_AddUserToProduction
                {
                    AddedByName = addedByUser.DisplayName,
                };
                FillProductionModel(model, production);
                RenderAndSendEmail(user, "ProductionMember-Added-ToNewMember", model, addedByUser.Email, addedByUser.DisplayName);
            }

            foreach (var currUserId in production.GetMembers().Where(u => (u != user.Id && u != addedByUser.Id)))
            {
                var currUser = Things.Get<UserThing>(currUserId);
                var sendEmail = (bool)currUser.GetSettingValue(UserSettings.NotifyWhenNewMember);
                if (!sendEmail)
                    continue;
                var model = new Email_NewMember
                                {
                                    MemberName = user.DisplayName,
                                    MemberEmail = user.Email,
                                };
                FillProductionModel(model, production);
                RenderAndSendEmail(currUser, "ProductionMember-Added-ToNewMember", model);
            }
            return new EmptyResult();
        }


        /// <summary>
        /// Sends the email to a new that was added to a new production.  Email is different for the person who uploaded than everyone else.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult NotifyForNewVideo(int videoId)
        {
            var video = Things.Get<VideoThing>(videoId);
            var project = video.FindAncestor<ProjectThing>();
            var creator = Things.Get<UserThing>(video.CreatedByUserId);
            var notes = video.ChildrenOfType<CommentThing>().Where(c => c.CreatedByUserId == video.CreatedByUserId && !(c is VideoCommentThing)).OrderBy(c => c.CreatedOn).Select(c=> c.Body).FirstOrDefault();//video notes are the first comment from the author (typically).  They aren't a videoCommentThing because videoCommentThings are tied to a timecode.

            foreach (var userId in project.GetMembers())
            {
                var user = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
                var sendEmail = (bool)user.GetSettingValue(UserSettings.NotifyWhenNewVideo);
                if (!sendEmail)
                    continue;

                var videoTitle = video.Title.Truncate(30, StringTruncating.EllipsisCharacter);

                var model = new Email_ProductionVideo_New
                                {
                                    Notes = notes,
                                    CreatedBy = creator.DisplayName,
                                    VideoTitle = videoTitle,
                                    WatchUrl = Url.Action2("Details", "Productions", new { id = project.Id, videoId = video.Id }, UrlType.AbsoluteHttps),
                                }.Once(m => FillProductionModel(m, project, video));

                if (video.CreatedByUserId == user.Id)
                {
                    RenderAndSendEmail(user, "ProductionVideo-Added-ToCreator", model);
                }
                else
                {
                    RenderAndSendEmail(user, "ProductionVideo-Added-ToNonCreators", model, creator.Email, creator.DisplayName);
                }
            }
            return new EmptyResult();
        }
        private void FillProductionModel(Email_ProductionBase model, ProjectThing production)
        {
            FillProductionModel(model, production, null);
        }

        private void FillProductionModel(Email_ProductionBase model, ProjectThing production, VideoThing video)
        {
            if ( video == null)
            {
                video = production.GetLatestVideo();
            }
            if ( video != null)
            {
                model.Thumbnails =
                    video.ChildrenOfType<VideoThumbnailThing>().Take(3).Select(t => new ViewModels.VideoThumbnail
                                                                                        {
                                                                                            Height = t.Height,
                                                                                            Width = t.Width,
                                                                                            Url = t.GetUrlHttps(),
                                                                                        }).ToArray();
            }
            model.ProductionTitle = production.Title;

            model.ProductionId = production.Id;
            if ( !model.ProductionUrl.HasChars())//certain custom urls where used and you don't want to overwrite them
                model.ProductionUrl = ProductionUrl(production);

            production.GetLogo().IfNotNull(l =>
            {
                model.LogoUrl = l.GetUrlHttps();
                model.LogoHeight = l.Height;
                model.LogoWidth = l.Width;
            });


        }

        public static string ProductionUrl(ProjectThing production)
        {
            if (production == null)
                throw new ArgumentNullException("production");
            string relativeUrl;
            if (production.VanityUrl.HasChars())
            {
                relativeUrl = production.VanityUrl;
            }
            else
            {
                relativeUrl = "productions/" + production.Id.ToInvariant();
            }
            return IoC.Get<IAppUrlPrefixes>().FromRelative(relativeUrl, UrlType.AbsoluteHttps);
        }



        /// <summary>
        /// If the user uploaded a new version to a production but the transcoding failed, that version gets deleted and we notify them here via email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult SendProductionVideoTranscodeError(int userId, int productionId, string originalFileName, TranscodeJobErrorCodes errorCode, string videoTitle)
        {
            var user = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
            var production = Things.Get<ProjectThing>(productionId);

            var model = new Email_ProductionVideo_EncodingError
            {
                Error = errorCode,
                UploadedFileName = originalFileName,
            };
            FillProductionModel(model, production);

            if ( errorCode == TranscodeJobErrorCodes.DurationTooLong)
            {
                RenderAndSendEmail(user, "ProductionVideo-EncodingError-TooLong", model);
            }
            else
            {
                RenderAndSendEmail(user, "ProductionVideo-EncodingError", model);                
            }
            return this.Empty();
        }

        /// <summary>
        /// Sends emails to necessary people after a video comment has been written.
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult NotifyForNewVideoComment(int commentId)
        {
            var comment = Things.Get<CommentThing>(commentId);
            var video = comment.FindAncestor<VideoThing>();
            var project = video.FindAncestor<ProjectThing>();
            var author = Things.Get<UserThing>(comment.CreatedByUserId);

            foreach (var userId in project.GetMembers().Where(u => u != comment.CreatedByUserId))
            {
                var user = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
                var sendEmail = (bool)user.GetSettingValue(UserSettings.NotifyWhenNewComment);
                if (!sendEmail)
                    continue;

                var model = new Email_ProductionVideoComment_New
                {
                    CommentBody = comment.Body,
                    CreatedBy = author.DisplayName,
                    VideoTitle = video.Title,
                    ViewUrl = Url.Action2("Details", "Productions", new { id = project.Id, videoId = video.Id }, UrlType.AbsoluteHttps),
                }.Once(m => FillProductionModel(m, project, video));
                RenderAndSendEmail(user, "ProductionVideoComment-New", model, author.Email, author.DisplayName);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// Sends emails to necessary people after a reply to a video comment has been written.
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult NotifyForNewVideoCommentReply(int replyId)
        {
            var reply = Things.Get<CommentReplyThing>(replyId);
            var replyAuthor = Things.Get<UserThing>(reply.CreatedByUserId);
            var comment = reply.FindAncestor<CommentThing>();
            var commentAuthor = Things.Get<UserThing>(comment.CreatedByUserId);
            var video = comment.FindAncestor<VideoThing>();
            var project = video.FindAncestor<ProjectThing>();
            var viewUrl = Url.Action2("Details", "Productions", new { id = project.Id, videoId = video.Id }, UrlType.AbsoluteHttps);

            foreach (var userId in project.GetMembers().Where(u => u != reply.CreatedByUserId))
            {
                var user = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
                var userWroteComment = user.Id == comment.CreatedByUserId;
                var sendEmail = (bool)user.GetSettingValue(UserSettings.NotifyWhenNewCommentReply) ||
                    (userWroteComment && (bool)user.GetSettingValue(UserSettings.NotifyWhenNewReplyToAuthoredComment));
                if (sendEmail)
                {

                    var replyModel = new Email_ProductionVideoCommentReply_New
                    {
                        CommentCreatedBy = commentAuthor.DisplayName,
                        CommentBody = comment.Body,
                        ReplyCreatedBy = replyAuthor.DisplayName,
                        ReplyBody = reply.Body,
                        VideoTitle = video.Title,
                        ViewUrl = viewUrl,
                        IsRecipientCommentCreator = userWroteComment,
                        IsCommentAndCommentReplySameCreator = commentAuthor.Id == replyAuthor.Id
                    };
                    FillProductionModel(replyModel, project, video);
                    RenderAndSendEmail(user, "ProductionVideoCommentReply-New", replyModel, replyAuthor.Email, replyAuthor.DisplayName);
                }
            }
            return new EmptyResult();
        }

        /// <summary>
        /// Sends emails to necessary people after a file has been uploaded to a project.
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [DeferredRequestOrLocalOnly]
        public ActionResult NotifyForNewProjectFile(int fileId)
        {
            var file = Things.Get<FileThing>(fileId);
            var project = file.FindAncestor<ProjectThing>();
            var author = Things.Get<UserThing>(file.CreatedByUserId);

            foreach (var userId in project.GetMembers().Where(u => u != file.CreatedByUserId))
            {
                var user = Things.GetOrNull<UserThing>(userId).EnsureExists(userId);
                var sendEmail = (bool)user.GetSettingValue(UserSettings.NotifyWhenNewFile);
                if (!sendEmail)
                    continue;


                var model = new Email_ProductionAsset_New
                {
                    CreatedBy = author.DisplayName,
                    DownloadUrl = Url.Action<ProductionFilesController>(c => c.Download(fileId), UrlType.AbsoluteHttps),
                    OriginalFileName = file.OriginalFileName,
                }.Once(m => FillProductionModel(m, project));
                RenderAndSendEmail(user, "ProductionAsset-New", model);
            }
            return new EmptyResult();
        }

        EmailResult RenderAndSendEmail(IUser to, string viewName, object model)
        {
            return RenderAndSendEmail(toUser: to, viewName: viewName, model: model);
        }

        EmailResult RenderAndSendEmail(IUser to, string viewName, object model, string replyToEmail, string replyToName)
        {
            return RenderAndSendEmail(toUser: to, viewName: viewName, model: model, replyToEmail: replyToEmail, replyToName: replyToName);
        }

        EmailResult RenderAndSendEmail(string toEmail = null, int? toUserId = null, IUser toUser = null, string viewName = null, string templateName = null, string subject = null, object model = null, string replyToName = null, string replyToEmail = null, MailAddress replyTo = null)
        {
            if (toUser != null)
            {                
                toUserId = toUser.Id;
                if (!toEmail.HasChars())
                {
                    toEmail = toUser.Email;
                }
            }

            var messageId = Guid.NewGuid();
            var emailResult = new EmailResult();
            emailResult.ViewName = viewName;
            emailResult.ViewData = base.ViewData;
            if (model != null)
                emailResult.ViewData.Model = model;
            //open tracking sticks a 1x1 transparent png that hits our server to indicate an email was opened
            emailResult.ViewData["IncludeOpenTracking"] = true;
            emailResult.ViewData["MessageId"] = messageId;
            emailResult.TempData = base.TempData;
            emailResult.ExecuteResult(this.ControllerContext);
            var message = OutgoingMail.ParseViewOutput(messageId, emailResult.Source);
            message.Id = messageId;
            message.IncludedTrackingImage = true;
            if ( subject.HasChars())
                message.Subject = subject;

            if (toEmail.HasChars())
                message.To = toEmail;

            message.ToUserId = toUserId;
            message.Template = templateName.CharsOr(viewName);

            if (replyTo != null)
            {
                if (!replyToEmail.HasChars())
                    replyToEmail = replyTo.Address;
                if (!replyToName.HasChars())
                    replyToName = replyTo.DisplayName;
            }

            var fromAddress = ConfigUtil.GetRequiredAppSetting("SendMailFromAddress");//all transactional emails currently go through this address
            var fromDefaultName = ConfigUtil.GetRequiredAppSetting("SendMailFromDefaultDisplayName");
            message.ReplyTo = string.Format("{0} <{1}>", replyToName.CharsOr(fromDefaultName), replyToEmail.CharsOr(fromAddress));
            message.From = string.Format("{0} <{1}>", replyToName.CharsOr(fromDefaultName), fromAddress);
            OutgoingMail.Send(message);
            return emailResult;
        }

    }


    /// <summary>
    /// Used to get the rendered string of a view.  I use this for email generation.  Also gets teh subject line for the email from the view.
    /// </summary>
    public class EmailResult : ViewResult
    {
        public string Source { get; set; }

        //almost all of this code is from the mvc framework
        public override void ExecuteResult(ControllerContext context)
        {
            //there will be no context when the action is executed programatically.  this makes a fake one
            if (context == null)
            {
                context = new ControllerContext
                {
                    RouteData = new System.Web.Routing.RouteData(),
                    HttpContext = HttpContextFactory.Current,
                };
                context.RouteData.Values["controller"] = "Email";
            }
            if (string.IsNullOrEmpty(this.ViewName))
            {
                this.ViewName = context.RouteData.GetRequiredString("action");
            }
            ViewEngineResult result = null;
            if (this.View == null)
            {
                result = this.FindView(context);
                this.View = result.View;
            }

            using (var output = new StringWriter())//just redirect output to a StringWriter
            {
                var viewContext = new ViewContext(context, this.View, this.ViewData, this.TempData, output);
                this.View.Render(viewContext, output);
                Source = output.ToString();
            }
            if (result != null)
            {
                result.ViewEngine.ReleaseView(context, this.View);
            }
        }



    }

}
