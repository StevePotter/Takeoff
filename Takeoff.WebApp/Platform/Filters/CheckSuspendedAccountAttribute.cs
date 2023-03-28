using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Takeoff.Data;
using Takeoff.Models;
using Mediascend.Web;
using MvcContrib;
using System.Security;
using Takeoff.Controllers;
using System.Diagnostics;

namespace Takeoff
{

    /// <summary>
    /// If the current user has an account and it's suspended due to expired trial or nonpayment or whatever, this gives them the proper response.
    /// </summary>
    public class CheckSuspendedAccountAttribute : Authorize2Attribute
    {

        public CheckSuspendedAccountAttribute()
        {
        }

        /// <summary>
        /// If set, this allows certain types of account status to pass through.  This is meant to be used by actions that resolve certain types of suspensions. 
        /// </summary>
        public AccountStatus[] StatusesToAllow { get; set; }

        protected override ActionResult GetUnauthorizedResult(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext.UserThing2();
            if (user == null)
                return null;

            var account = user.Account;
            if (account == null)
                return null;

            if (account.IsSuspended() && !(StatusesToAllow.HasItems() && StatusesToAllow.Contains(account.Status)))
            {
                var result = CreateSuspendedAccountResult(filterContext, true, account);
                if (result != null)
                    return result;
            }

            return null;
        }


        protected virtual ActionResult CreateSuspendedAccountResult(AuthorizationContext filterContext, bool isUsersAccount, IAccount account)
        {
            var request = filterContext.HttpContext.Request;
            var status = account.Status;
            //give them a vague error not to embarrass account holder
            if (!isUsersAccount)
            {
                return CreateForbiddenResult(filterContext, "Account-Suspended-NotOwner", ErrorCodes.AccountProblem, ErrorCodes.AccountProblemDescription);
            }

            if (status == AccountStatus.TrialExpired || status == AccountStatus.Pastdue || status == AccountStatus.ExpiredNonpayment)
            {
                var url = new UrlHelper(filterContext.RequestContext).Action<SignupController>(c => c.Subscribe(request.Url.OriginalString, null), UrlType.AbsoluteHttps);
                if (request.IsWebPageRequestOrNonAjaxFormPost())
                {
                    return new RedirectResult(url);
                }
                return new NonHtmlErrorResponse
                {
                    ErrorCode = ErrorCodes.BillingIssue,
                    ErrorDescription = "There is a billing issue with your account",
                    ResolveUrl = url,
                    StatusCode = HttpStatusCode.Forbidden
                };
            }

            //handle suspensions with diffrent messaging
            Debug.Assert(status == AccountStatus.Suspended);
            return CreateForbiddenResult(filterContext, "Account-Suspended", ErrorCodes.AccountSuspended, ErrorCodes.AccountSuspendedDescription);
        }


    }


}