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
using Takeoff.Controllers;

namespace Takeoff
{

    /// <summary>
    /// Ensures that the action can only be executed by a user whose email has been verified.
    /// </summary>
    public class RequiresVerificationAttribute : Authorize2Attribute
    {
        protected override ActionResult GetUnauthorizedResult(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext.UserThing2();
            if (user == null || user.IsVerified || !user.Email.HasChars() || !user.VerificationKey.HasChars())
                return null;

            var request = filterContext.HttpContext.Request;
            if (request.IsWebPageRequestOrNonAjaxFormPost())//contenttype checks ensure xml, json, etc are excluded
            {
                return new ViewResult
                {
                    ViewName = "RequiresVerification",
                };
            }

            var url = new UrlHelper(filterContext.RequestContext).Action<AccountController>(c => c.Verify(null), UrlType.AbsoluteHttps);
            return new NonHtmlErrorResponse
            {
                ResolveUrl = url,
                ErrorCode = ErrorCodes.UnverifiedEmail,
                ErrorDescription = ErrorCodes.UnverifiedEmailDescription,
                StatusCode = HttpStatusCode.Forbidden,
            };

        }

    }

}
