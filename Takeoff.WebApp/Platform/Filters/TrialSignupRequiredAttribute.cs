using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Takeoff.Models;
using Mediascend.Web;
using MvcContrib;
using System.Security;
using Takeoff.Data;
using Takeoff.Controllers;

namespace Takeoff
{

    /// <summary>
    /// If the user is in a trial (Account.Status == TrialAnonymous), this requires they have their basic user data set.  Used for actions that can't be done anonymously.
    /// </summary>
    public class TrialSignupRequiredAttribute : Authorize2Attribute
    {
        protected override ActionResult GetUnauthorizedResult(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext.UserThing2();
            if ( user == null)
                return null;
            var account = user.Account;
            if ( account == null || account.Status != AccountStatus.TrialAnonymous)
                return null;
            var request = filterContext.HttpContext.Request;
            if (request.IsWebPageRequestOrNonAjaxFormPost())
            {
                return new ViewResult
                           {
                               ViewData = SetViewDataAttribute.FillViewData(new ViewDataDictionary(), filterContext.HttpContext, filterContext.ActionDescriptor),
                               ViewName = "Account-Trial-SignupRequired",
                           };
            }

            return new NonHtmlErrorResponse
            {
                ResolveUrl = new UrlHelper(filterContext.RequestContext).Action<SignupController>(c => c.Index(SignupType.TrialUserInfoRequested, null, null), UrlType.AbsoluteHttps),
                ErrorCode = ErrorCodes.BasicTrialInformationRequired,
                ErrorDescription = ErrorCodes.BasicTrialInformationRequiredDescription,
                StatusCode = HttpStatusCode.Forbidden,
            };

        }

    }

    public class Trial2ForbiddenAttribute : Authorize2Attribute
    {
        protected override ActionResult GetUnauthorizedResult(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext.UserThing2();
            if (user == null)
                return null;
            var account = user.Account;
            if (account == null || account.Status != AccountStatus.Trial2)
                return null;
            var request = filterContext.HttpContext.Request;
            if (request.IsWebPageRequestOrNonAjaxFormPost())
            {
                
                return new ViewResult
                {
                    ViewData = SetViewDataAttribute.FillViewData(new ViewDataDictionary(), filterContext.HttpContext, filterContext.ActionDescriptor),
                    ViewName = "Account-Trial-PurchaseRequired",
                };
            }

            return new NonHtmlErrorResponse
            {
                ResolveUrl = new UrlHelper(filterContext.RequestContext).Action<SignupController>(c => c.Subscribe(null, null), UrlType.AbsoluteHttps),
                ErrorCode = ErrorCodes.BasicTrialInformationRequired,
                ErrorDescription = ErrorCodes.BasicTrialInformationRequiredDescription,
                StatusCode = HttpStatusCode.Forbidden,
            };

        }

    }
}
