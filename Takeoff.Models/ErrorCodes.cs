using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Takeoff.Models
{

    public static class ErrorCodes
    {

        public static string UserCannotSignupBecauseTheyAreLoggedIn = "UserCannotSignupBecauseTheyAreLoggedIn";
        public static string UserCannotSignupBecauseTheyAreLoggedInDescription = "You are currently signed in and thus don't need to register.";


        public static string UserCannotSignupBecauseTheyAlreadyDid = "UserCannotSignupBecauseTheyAlreadyDid";
        public static string UserCannotSignupBecauseTheyAlreadyDidDescription = "You are currently signed up and don't need to do it again.";

       
        /// <summary>
        /// The action couldn't be performed because the user is in a demo account.  If they upgrade to a regular account, it will likely work.
        /// </summary>
        public static string DemoForbidden = "DemoForbidden";
        public static string DemoForbiddenDescription = "Demos are not allowed.";

        public static string NoPermission = "NoPermission";
        public static string NoPermissionDescription = "You don't have permission to do this.";

        /// <summary>
        /// The action couldn't be performed because the user didn't verify their email address.
        /// </summary>
        public static string UnverifiedEmail = "UnverifiedEmail";
        public static string UnverifiedEmailDescription = "You must verify your email address to do this.";


        /// <summary>
        /// The action couldn't be performed because the user didn't have basic information needed for trials.
        /// </summary>
        public static string BasicTrialInformationRequired = "BasicTrialInformationRequired";
        public static string BasicTrialInformationRequiredDescription = "Basic user information - name, email, pw - is required for this.";

        
        /// <summary>
        /// The user tried to add a video but the plan's # of videos is already maxed out.
        /// </summary>
        public static string AccountPlanVideoNumLimit = "AccountPlanVideoNumLimit";


        

        /// <summary>
        /// The action couldn't be completed because the user wasn't logged in.
        /// </summary>
        public static string NotLoggedIn = "NotLoggedIn";

        /// <summary>
        /// The target of the action, such as a file or comment, couldn't be found.
        /// </summary>
        public static string NotFound = "NotFound";
        public static string NotFoundDescription = "The data you requested could not be found.";

        /// <summary>
        /// User doesn't have access to perform the given action on the target.
        /// </summary>
        public static string NoAccess = "NoAccess";

        /// <summary>
        /// The action couldn't be performed because the user's free trial account expired, or they are past due on their account.  They have to resolve that.
        /// </summary>
        public static string BillingIssue = "BillingIssue";

        /// <summary>
        /// The action couldn't be performed because there was a problem with account associated with the target.  This is intended to be used when the account doesn't belong to the current user, so there's nothing to do but give a friendly error message and wait.
        /// </summary>
        public static string AccountProblem = "AccountProblem";
        public static string AccountProblemDescription = "There is a problem with the account.  The account owner needs to contact us.";

        public static string MembershipRequestsDisabled = "MembershipRequestsDisabled";
        public static string MembershipRequestsDisabledDescription = "Access requests are disabled.";

        public static string MembershipRequestSent = "MembershipRequestSent";
        public static string MembershipRequestSentDescription = "Your request for membership has been sent.";

        public static string NotMemberCanRequest = "NotMemberCanRequest";
        public static string NotMemberCanRequestDescription = "You aren't a member but you can request membership.";

        public static string NotMemberMustAcceptInvitation = "NotMemberMustAcceptInvitation";
        public static string NotMemberMustAcceptInvitationDescription = "You must accept the invitation before you can become a member.";

        public static string GuestAccessForbidden = "GuestAccessForbidden";
        public static string GuestAccessForbiddenDescription = "Guests are not allowed to login via a simple password.  Try creating a Takeoff account instead.";

        /// <summary>
        /// The action couldn't be performed because the account was suspended for some reason.
        /// </summary>
        public static string AccountSuspended = "AccountSuspended";
        public static string AccountSuspendedDescription = "Your account has been suspended.  Please contact us.";

        /// <summary>
        /// Indicates the account reached its max number of videos.
        /// </summary>
        public static string AccountVideoLimitReached = "AccountVideoLimitReached";

        /// <summary>
        /// The user must be a Takeoff client.
        /// </summary>
        public static string AccountRequired = "AccountRequired";
        public static string AccountRequiredDescription = "You must be a Takeoff client and have an account plan to do this.";

        /// <summary>
        /// The person has an account but it has an invalid status.
        /// </summary>
        public static string AccountStatusInvalid = "AccountStatusInvalid";
        public static string AccountStatusInvalidDescription = "You must be a Takeoff client and have an account plan to do this.";

        public static string GuestForbidden = "GuestForbidden";
        public static string GuestForbiddenDescription = "Guest users are not allowed.  You must be a member or client to continue.";

        public static string InvalidInput = "InvalidInput";
        public static string InvalidInputDescription = "The input was invalid.  See the Errors property.";

        public static string VideoNotDownloadable = "VideoNotDownloadable";
        public static string VideoNotDownloadableDescription = "The video is not downloadable.";


        //public static string Description(Expression<Func<object>> modelPropertyExpr)
        //{
        //    var property = modelPropertyExpr.Body.CastTo<MemberExpression>().Member;
        //    return property.Attribute<DescriptionAttribute>().MapIfNotNull(a => a.Description);
        //}
    }
}