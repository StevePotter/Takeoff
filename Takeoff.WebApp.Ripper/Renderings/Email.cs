using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CommandLine;
using Takeoff.Controllers;
using System.IO;
using System.Web;
using Moq;
using System.Security.Principal;
using System.Collections;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Specialized;
using System.Web.WebPages;
using System.Web.Hosting;
using System.Reflection;
using Ninject;
using System.Threading;
using System.Linq.Expressions;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Resources;
using Takeoff.ThingRepositories;
using Takeoff.ViewModels;

namespace Takeoff.WebApp.Ripper
{
    partial class Program
    {


        private static void RenderViewsForEmail()
        {

            new EmailRenderer
            {
                View = typeof(Views.Email.ActivityDigest).Name,
                Model = new Email_ActivityDigest
                {
                    ActivityPerProduction = new Email_ActivityDigest.ProductionActivity[]
                                                            {
                                                                new Email_ActivityDigest.ProductionActivity
                                                                    {
                                                                        HtmlChanges = new string[]{"12/5/2011 12:18 AM: You added a video called 'Version 9'","2/4/2011 11:11 PM: You added a video called 'Version 8'"},
                                                                        PlainTextChanges = new string[]{"12/5/2011 12:18 AM: You added a video called 'Version 9'","2/4/2011 11:11 PM: You added a video called 'Version 8'"},
                                                                        ProductionId = 45,
                                                                        ProductionTitle = "NY Video",
                                                                        ProductionUrl = "Productions_Details.html",
                                                                        LogoUrl = "rip-assets/nyvideo.png",
                                                                        LogoHeight = 70,
                                                                        LogoWidth = 58,
                                                                    },
                                                                    new Email_ActivityDigest.ProductionActivity
                                                                    {
                                                                        HtmlChanges = new string[]{"Steve Potter wrote a comment - 'ok'"},
                                                                        PlainTextChanges = new string[]{"Steve Potter wrote a comment - 'ok'"},
                                                                        ProductionId = 45,
                                                                        ProductionTitle = "Takeoff",
                                                                        ProductionUrl = "Productions_Details.html",
                                                                        LogoUrl = "rip-assets/takeoff.png",
                                                                        LogoHeight = 60,
                                                                        LogoWidth = 190,
                                                                    }
                                                            },
                    PeriodEnd = DateTime.Now,
                    PeriodStart = DateTime.Now.AddDays(-1),
                },
            }.Render();


            new EmailRenderer
            {
                View = typeof(Views.Email.TrialExpired).Name,
            }.Render();

            new EmailRenderer
            {
                View = typeof(Views.Email.TrialExpiresTomorrow).Name,
            }.Render();

            new EmailRenderer
            {
                View = typeof(Views.Email.TrialExpiresInDays).Name,
                Model = new Email_TrialExpiresInDays
                {
                    DaysLeft = 6,
                    ExpiresOn = DateTime.Parse("12/5/2011")
                }
            }.Render();

            var newVideo = new Email_ProductionVideo_New
            {
                CreatedBy = "Bill Stuart",
                Notes = "I did a bunch of cool new effects over the green screen.  Hope you dig it.",
                VideoTitle = "Motion GFX Updates",
                WatchUrl = "Productions_Details.html",
            }.FillProductionData();

            new EmailRenderer
            {
                View = "ProductionVideo-Added-ToNonCreators",
                Model = newVideo,
            }.Render();

            new EmailRenderer
            {
                View = "ProductionVideo-Added-ToCreator",
                Model = newVideo,
            }.Render();



            new EmailRenderer
            {
                View = "ProductionVideoComment-New",
                Model = new Email_ProductionVideoComment_New
                {
                    CreatedBy = "Bill Stuart",
                    CommentBody = "We wanted to use the latter half of this video, starting around the 23 second mark to avoid staring at the backside of the female in the white pants.",
                    VideoTitle = "Motion GFX Updates",
                    ViewUrl = "Productions_Details.html",
                }.FillProductionData(),
            }.Render();


            Func<Email_AddUserToProduction> userAddedModel = () => new Email_AddUserToProduction
            {
                AddedByName = User.SubscribedFreelance.DisplayName,
                AddedByEmail = User.SubscribedFreelance.Email,
            }.FillProductionData();
            new EmailRenderer
            {
                View = "ProductionMember-Added-ToNewMember",
                Model = userAddedModel(),
            }.Render();


            new EmailRenderer
            {
                View = "ProductionMember-Added-ToOtherMembers",
                Model = new Email_NewMember
                {
                    MemberName = User.Guest.DisplayName,
                    MemberEmail = User.Guest.Email,
                }.FillProductionData()
            }.Render();

            new EmailRenderer
            {
                View = "ProductionMember-Invited-NewUser",
                Model = userAddedModel().Once(m => m.Note = "Hey buddy, this is the new system I was telling you about."),
            }.Render();

            new EmailRenderer
            {
                View = "ProductionMember-Invited-ExistingUser",
                Model = userAddedModel().Once(m => m.Note = "Let's get this thing done!"),
            }.Render();

            new EmailRenderer
            {
                View = "ProductionMember-RequestByNonMember",
                Model = new Email_MembershipRequestToProduction
                {
                    RequestToEmail = User.SubscribedFreelance.Email,
                    RequestToName = User.SubscribedFreelance.DisplayName,
                    RequestFromEmail = User.Guest.Email,
                    RequestFromName = User.Guest.DisplayName,
                    RequestUrl = "",//this.Url.Action<MembershipRequestsController>(r => r.Details(requestId), UrlType.AbsoluteHttps)
                    Note = "Let me in!"
                }.FillProductionData()
            }.Render();

            new EmailRenderer
            {
                View = "ProductionVideo-EncodingError",
                Model = new Email_ProductionVideo_EncodingError
                {
                    VideoTitle = "Scene 2, Version 3",
                    Error = Takeoff.Transcoder.TranscodeJobErrorCodes.NotAVideo,
                    UploadedFileName = "video.avi",
                }.FillProductionData().Once(m => m.Thumbnails = null)
            }.Render();


            new EmailRenderer
            {
                View = "ProductionVideo-EncodingError-TooLong",
                Model = new Email_ProductionVideo_EncodingError
                {
                    VideoTitle = "Scene 2, Version 3",
                    Error = Takeoff.Transcoder.TranscodeJobErrorCodes.DurationTooLong,
                    UploadedFileName = "video.avi",
                }.FillProductionData().Once(m => m.Thumbnails = null)
            }.Render();


            new EmailRenderer
            {
                View = "ProductionAsset-New",
                Model = new Email_ProductionAsset_New
                {
                    DownloadUrl = "rip-assets/asset-sample.docx",
                    OriginalFileName = "script.doc",
                    CreatedBy = User.SubscribedFreelance.DisplayName,
                }.FillProductionData()
            }.Render();


            var replyModel = new Email_ProductionVideoCommentReply_New
            {
                CommentCreatedBy = User.SubscribedFreelance.DisplayName,
                CommentBody =
                    "We wanted to use the latter half of this video, starting around the 23 second mark to avoid staring at the backside of the female in the white pants.",
                ReplyCreatedBy = User.Guest.DisplayName,
                ReplyBody = "I think that's tasteless dude.",
                VideoTitle = "Motion GFX Updates",
                ViewUrl = "Productions_Details.html",
            }.FillProductionData();
            new EmailRenderer
            {
                View = "ProductionVideoCommentReply-New",
                Model = replyModel,
            }.Render();

            replyModel.IsCommentAndCommentReplySameCreator = true;
            new EmailRenderer
            {
                View = "ProductionVideoCommentReply-New",
                Model = replyModel,
                Scenario = "SameAuthor",
                AddViewNameToFileName = true,
            }.Render();


            replyModel.IsCommentAndCommentReplySameCreator = false;
            replyModel.IsRecipientCommentCreator = true;
            new EmailRenderer
            {
                View = "ProductionVideoCommentReply-New",
                Model = replyModel,
                Scenario = "RecipientWroteComment",
                AddViewNameToFileName = true,
            }.Render();


            new EmailRenderer
            {
                View = "User-Signup-MustVerify",
                Model = new Email_User_Signup_MustVerify
                {
                    VerifyUrl = "Account_Verify_Verify-Success.html",
                },
            }.Render();

            new EmailRenderer
            {
                View = "User-Verify",
                Model = new Email_User_Verify
                {
                    VerifyUrl = "Account_Verify_Verify-Success.html",
                },
            }.Render();

            new EmailRenderer
            {
                View = "User-PasswordReset",
                Model = new Email_User_PasswordReset
                {
                    ResetUrl = "Account_PasswordReset.html",
                },
            }.Render();


        }

    }
}
