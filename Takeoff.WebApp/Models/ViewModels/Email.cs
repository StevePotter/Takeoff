using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Data;
using Takeoff.Transcoder;

namespace Takeoff.ViewModels
{

    public class Email_AddUserToProduction : Email_ProductionBase
    {

        public string AddedByName { get; set; }

        public string AddedByEmail { get; set;  }

        public string Note { get; set; }

    }

    public class Email_MembershipRequestToProduction : Email_ProductionBase
    {
        /// <summary>
        /// The person asking permission to be a member.
        /// </summary>
        public string RequestFromName { get; set; }

        public string RequestFromEmail { get; set; }

        /// <summary>
        /// The person who can grant membership.
        /// </summary>
        public string RequestToName { get; set; }

        public string RequestToEmail { get; set; }

        public string Note { get; set; }

        public string RequestUrl { get; set; }
    }


    public class Email_NewMember : Email_ProductionBase
    {

        public string MemberName { get; set; }

        public string MemberEmail { get; set; }

        public string Note { get; set; }

    }


    public class Email_ActivityDigest
    {
        /// <summary>
        /// The beginning, in the user's local time, of the activity period we are reporting.
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// The end, in the user's local time, of the activity period we are reporting.
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        public ProductionActivity[] ActivityPerProduction { get; set; }

        public class ProductionActivity
        {
            public string ProductionTitle { get; set; }
            public string ProductionUrl { get; set; }
            public int ProductionId { get; set; }

            public int LogoWidth { get; set; }
            public int LogoHeight { get; set; }
            public string LogoUrl { get; set; }

            public string[] HtmlChanges { get; set; }
            public string[] PlainTextChanges { get; set; }
        }
    }


    public class Email_TrialExpiresInDays
    {
        public int DaysLeft { get; set; }
        /// <summary>
        /// The time, in user's local timezone, when it expires.
        /// </summary>
        public DateTime ExpiresOn { get; set; }
    }

    /// <summary>
    /// Base class for emails that show a production title, link, and thumbnails.
    /// </summary>
    public class Email_ProductionBase
    {
        public string ProductionTitle { get; set; }
        public string ProductionUrl { get; set; }
        public int ProductionId { get; set; }

        public int LogoWidth { get; set; }
        public int LogoHeight { get; set; }
        public string LogoUrl { get; set; }

        /// <summary>
        /// Thumbnails to show across the top;
        /// </summary>
        public VideoThumbnail[] Thumbnails { get; set; }
    }


    public class Email_ProductionVideo_New: Email_ProductionBase
    {
        public string VideoTitle { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public string WatchUrl { get; set; }
    }

    public class Email_ProductionVideoComment_New : Email_ProductionBase
    {
        public string VideoTitle { get; set; }
        public string CommentBody { get; set; }
        public string CreatedBy { get; set; }
        public string ViewUrl { get; set; }
    }

    public class Email_ProductionVideoCommentReply_New : Email_ProductionBase
    {
        public string VideoTitle { get; set; }
        public string CommentBody { get; set; }
        public string CommentCreatedBy { get; set; }
        public string ReplyBody { get; set; }
        public string ReplyCreatedBy { get; set; }
        public bool IsRecipientCommentCreator { get; set; }
        public bool IsCommentAndCommentReplySameCreator { get; set; }
        public string ViewUrl { get; set; }
    }


    public class Email_ProductionAsset_New : Email_ProductionBase
    {
        public string OriginalFileName { get; set; }
        public string DownloadUrl { get; set; }
        public string CreatedBy { get; set; }
    }



    public class Email_ProductionVideo_EncodingError : Email_ProductionBase
    {
        public string VideoTitle { get; set; }
        public string UploadedFileName { get; set; }
        public TranscodeJobErrorCodes Error { get; set; }
    }

    public class Email_User_Signup_MustVerify
    {
        public string VerifyUrl { get; set; }
    }

    public class Email_User_Verify
    {
        public string VerifyUrl { get; set; }
    }


    public class Email_User_PasswordReset
    {
        public string ResetUrl { get; set; }
    }
}