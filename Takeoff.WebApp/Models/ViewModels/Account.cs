using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;
using Takeoff.Data;

namespace Takeoff.ViewModels
{
    /// <summary>
    /// for /login
    /// </summary>
    public class Account_Login
    {
        /// <summary>
        /// A custom url to redirect them to once they log in.
        /// </summary>
        public string ReturnUrl { get; set; }
        public string Heading { get; set; }

        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Email { get; set; }
        
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Password { get; set; }
        /// <summary>
        /// The number of hours from UTC, provided by javascript, for the user.  This allows us to convert the database's UTC times into local ones.
        /// </summary>
        public int? TimezoneOffset { get; set; }

        public bool RememberMe { get; set; }

        public bool UseAjax { get; set; }
    }


    public class Account_Subscription : BillingInfo
    {

    }

    public class Account_Notifications  
    {
        public EmailDigestFrequency DigestEmailFrequency { get; set; }

        public bool NotifyWhenNewVideo { get; set; }
        public bool NotifyWhenNewComment { get; set; }
        public bool NotifyWhenNewCommentReply { get; set; }
        public bool NotifyWhenNewReplyToAuthoredComment { get; set; }
        public bool NotifyWhenNewMember { get; set; }
        public bool NotifyWhenNewFile { get; set; }

        public bool NotifyForNewFeatures { get; set; }
        public bool NotifyForMaintenance { get; set; }
        public bool NotifyForPlanChanges { get; set; }
        public bool NotifyForSpecials { get; set; }
    
    }

    public class Account_Privacy
    {
        public bool EnableMembershipRequests { get; set; }
        public bool EnableInvitations { get; set; }

        public AutoResponse[] AutoResponses { get; set; }

        /// <summary>
        /// Indicates an automatic response to an invitation or request by someone for production membership.
        /// 
        /// That the only thing that can never happen now is automatically accepting requests from someone to join productions, since this could create problems for people.  This could always be changed in the future.
        /// </summary>
        public class AutoResponse
        {
            /// <summary>
            /// The ID of this auto response record.
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// If true, invitations are being accepted.  Otherwise they are being rejected.
            /// </summary>
            public bool Accept { get; set; }
            /// <summary>
            /// If true, invitations are are accepted or rejected.  If false, requests from someone are ignored.
            /// </summary>
            public bool IsInvitation { get; set; }
            /// <summary>
            /// The name of the person to automatically respond to.
            /// </summary>
            public string TargetUserName { get; set; }
            /// <summary>
            /// The email of the person to automatically respond to.
            /// </summary>
            public string TargetUserEmail { get; set; }
        }
    }

    public class Account_Verify
    {

        [LocalizedDisplayName("Account_ViewModel_Email")]
        [Email(ErrorMessageResourceName = "Validation_InvalidEmail", ErrorMessageResourceType = typeof(Resources.Strings))]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Email { get; set; }

        /// <summary>
        /// Optional url that started the signup.  This way they can be sent to it when the signup process is complete.
        /// </summary>
        public string ReturnUrl { get; set; }
       
        /// <summary>
        /// The key required to authenticate the email.
        /// </summary>
        public string VerificationKey { get; set; }
    }


    public class Account_Verify_Request
    {

        [LocalizedDisplayName("Account_ViewModel_Email")]
        [Email(ErrorMessageResourceName = "Validation_InvalidEmail", ErrorMessageResourceType = typeof(Resources.Strings))]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Email { get; set; }

        /// <summary>
        /// The number of hours from UTC, provided by javascript, for the user.  This allows us to convert the database's UTC times into local ones.
        /// </summary>
        public int? TimezoneOffset { get; set; }

    }



    public class Account_MainInfo
    {

        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string LastName { get; set; }

        [Email(ErrorMessageResourceName = "Validation_InvalidEmail", ErrorMessageResourceType = typeof(Resources.Strings))]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string CurrentPassword { get; set; }

        [DisplayName("Optional New Password")]
        public string NewPassword { get; set; }

    }

    public class Account_Logo
    {
        public string CurrentLogoUrl { get; set; }

        public HttpPostedFileBase Logo { get; set; }

    }

    public class Account_NecessaryInfo
    {
        public string ReturnUrl { get; set; }

        /// <summary>
        /// The number of hours from UTC, provided by javascript, for the user.  This allows us to convert the database's UTC times into local ones.
        /// </summary>
        public int? TimezoneOffset { get; set; }

        [LocalizedDisplayName("Account_ViewModel_FirstName")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string FirstName { get; set; }

        [LocalizedDisplayName("Account_ViewModel_LastName")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string LastName { get; set; }

        [LocalizedDisplayName("Account_ViewModel_Password")]
        public string Password { get; set; }

        /// <summary>
        /// The value of the "how did you hear about us" dropdown.
        /// </summary>
        public string SignupSource { get; set; }

        /// <summary>
        /// If they choose "Other" for SignupSource, this is the custom entered value.
        /// </summary>
        public string SignupSourceOther { get; set; }

    }


    public class Account_PasswordReset
    {
        /// <summary>
        /// A secret key that is used to verify the reset request is valid.
        /// </summary>
        public string ResetKey { get; set; }

        /// <summary>
        /// The email address cooresponding to the account in question.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Their new password.
        /// </summary>
        [LocalizedDisplayName("PasswordReset_PasswordLabel")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string NewPassword { get; set; }

    }


    /// <summary>
    /// Not a page-specific model.  Gives a summary of account usage.
    /// </summary>
    public class AccountUsage
    {
        public int VideosUsed { get; set; }
        public int? VideosAllowed { get; set; }
        public FileSize AssetsTotalSize { get; set; }
        public FileSize? AssetsAllowed { get; set; }
        public int AssetFilesCount { get; set; }

        //public int AssetsPercentUsed
        //{
        //    get { }
        //}

    }


    public class Account_LimitReached
    {
        public string LimitationMessage { get; set; }
    }


}
